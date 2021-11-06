using AutoMapper;
using AWSHelpers.S3.Interfaces;
using AWSHelpers.SQS.Interfaces;
using Core.Common;
using Core.Configuration;
using Core.Web.Entities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;
using SPGovernmentData.Application.Entities.DTOs;
using SPGovernmentData.Application.Entities.Utils;
using SPGovernmentData.Application.Services.Interfaces;
using SPGovernmentData.Domain.Entities;
using SPGovernmentData.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace SPGovernmentData.Application.Services.Implementations
{
    public class ParserService : GovDataUtils, IParserService
    {
        private readonly Parser _parser;
        private readonly HttpClient _client;
        private readonly IMapper _mapper;
        private readonly IS3Helper _S3Helper;
        private readonly ISQSHelper _SQSHelper;
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly IDatasetRepository _datasetRepository;
        private readonly IDataSourceRepository _dataSourceRepository;
        private readonly IUrlExtractedRepository _extractedRepository;
        private readonly ITagRepository _tagRepository;
        private readonly IDatasetAditionalInformationRepository _datasetAditionalInformation;
        private readonly IDataSourceAditionalInformationRepository _dataSourceAditionalInformation;

        public ParserService(IDatasetRepository datasetRepository, HttpClient client,
                             IDatasetAditionalInformationRepository datasetAditionalInformation,
                             ITagRepository tagRepository, IDataSourceAditionalInformationRepository dataSourceAditionalInformation,
                             IDataSourceRepository dataSourceRepository, IHostingEnvironment hostingEnvironment,
                             IUrlExtractedRepository extractedRepository, ISQSHelper sQSHelper, IOptions<Configs> options, IMapper mapper, IS3Helper s3Helper)
        {
            _client = client;
            _datasetRepository = datasetRepository;
            _datasetAditionalInformation = datasetAditionalInformation;
            _tagRepository = tagRepository;
            _dataSourceAditionalInformation = dataSourceAditionalInformation;
            _dataSourceRepository = dataSourceRepository;
            _hostingEnvironment = hostingEnvironment;
            _extractedRepository = extractedRepository;
            _SQSHelper = sQSHelper;
            _S3Helper = s3Helper;
            _parser = options.Value.Parser;
            _mapper = mapper;
        }

        public async Task<List<DatasetDTO>> ParserUrlToDataset(string search)
        {
            List<DatasetDTO> datasetDTOs = new();
            List<Task<Dataset>> datasetsTasks = new();
            List<UrlExtractedDTO> urls = await GetUrlsParser(search);

            if (urls != null && urls.Any())
            {
                foreach (UrlExtractedDTO url in urls)
                {
                    HtmlString html = await _client.GetResponseHtmlAsync(url.Url);
                    if (!html.IsNullOrEmpty)
                    {
                        datasetsTasks.Add(ExtractGeneralInfo(html, search));
                    }
                }
            }

            if (datasetsTasks.Any())
            {
                var datasets = await Task.WhenAll(datasetsTasks);
                datasetDTOs = _mapper.Map<List<DatasetDTO>>(datasets);
            }

            return datasetDTOs;
        }

        private async Task<Dataset> ExtractGeneralInfo(HtmlString html, string search)
        {
            DatasetAditionalInformation aditionalInformation = await ExtractDatasetAddionalInformation(html);
            Dataset dataset = await ExtractDatasetInformation(html, aditionalInformation.Id);
            await ExtractTagInformation(html, dataset.Id);
            await ExtractDataSourcesInformation(html, dataset, search);
            return dataset;
        }

        private async Task<List<DataSource>> ExtractDataSourcesInformation(HtmlString html, Dataset dataset, string search)
        {
            List<string> titles = html.ExtractListInfo(".//div[@id='content']//article[@class='module']//ul[@class='resource-list']/li[@class='resource-item']//a[@class='heading']");
            List<string> urls = html.ExtractListInfoAttributes(".//div[@id='content']//article[@class='module']//ul[@class='resource-list']/li[@class='resource-item']//a[@class='heading']", "href", false);
            List<DataSource> dataSources = new();

            if (titles != null && urls != null && titles.Count() == urls.Count())
            {
                foreach (int indice in Enumerable.Range(0, titles.Count))
                {
                    string urlHtml = _baseUrlGov.CombineUrl(urls[indice]);
                    var addionalInformation = await ExtractDatasourceAddionalInformation(urlHtml);

                    if (addionalInformation != null)
                    {
                        string downloadPath = await DownloadDataSourceDocument(addionalInformation.UrlFile, search);
                        DataSource dataSource = new(titles[indice], urlHtml, downloadPath, addionalInformation.Id, dataset.Id);
                        var dataSourceInserted = await _dataSourceRepository.InsertAsync(dataSource);
                        dataSources.Add(dataSourceInserted);
                    }
                }
            }
            return dataSources;
        }

        private async Task<List<Tag>> ExtractTagInformation(HtmlString html, int? datasetId)
        {
            List<string> tagsName = html.ExtractListInfo(".//div[@id='content']//section[@class='tags']/ul[@class='tag-list well']/li/a[@class='tag']");
            List<Tag> tags = new();

            if (tagsName != null && tagsName.Any())
            {
                foreach (string tagName in tagsName)
                {
                    Tag tag = new(tagName, datasetId);
                    Tag tagInserted = await _tagRepository.InsertAsync(tag);

                    if (tagInserted != null)
                    {
                        tags.Add(tagInserted);
                    }
                }
            }
            return tags;
        }

        private async Task<Dataset> ExtractDatasetInformation(HtmlString html, int? aditionalInformationId)
        {
            string title = html.ExtractSingleInfo(".//div[@id='content']//article[@class='module']//h1");
            string description = html.ExtractSingleInfo(".//div[@id='content']//div[@class='notes embedded-content']//p");
            string license = html.ExtractSingleInfo(".//div[@id='content']//section[@class='module module-narrow module-shallow license']/p[@class='module-content']/a");
            string organization = html.ExtractSingleInfo(".//div[@id='content']//div[@class='module module-narrow module-shallow context-info']/section[@class='module-content']/p");
            int followers = html.ExtractSingleInfo(".//div[@id='content']//div[@class='module-content']/div[@class='nums']/dl/dd/span").TryConvertStringToInt();

            Dataset dataset = new(title, description, followers, organization, license, aditionalInformationId);
            Dataset datasetInserted = await _datasetRepository.InsertAsync(dataset);

            return datasetInserted;
        }

        private async Task<DataSourceAditionalInformation> ExtractDatasourceAddionalInformation(string url)
        {
            DataSourceAditionalInformation aditionalInformationInserted = new();
            HtmlString html = await _client.GetResponseHtmlAsync(url);

            if (!html.IsNullOrEmpty)
            {
                string urlFile = html.ExtractSingleInfoAttribute("//div[@id='content']//div[@class='module-content']/p[@class='muted ellipsis']/a", "href", false);
                Dictionary<string, string> extracted = html.ExtractTableInfo(".//div[@id='content']//div[@class='module-content']/table");

                if (extracted.Any())
                {
                    DateTime? lastUpdate = extracted.GetValueByKey("ÚLTIMA ATUALIZAÇÃO").TryConvertDatetime("MMMM d, yyyy", "pt-BR");
                    DateTime? creationDate = extracted.GetValueByKey("CRIADO").TryConvertDatetime("MMMM d, yyyy", "pt-BR");
                    string format = extracted.GetValueByKey("FORMATO");
                    string license = extracted.GetValueByKey("LICENÇA");
                    string created = extracted.GetValueByKey("CREATED");
                    bool hasViews = extracted.GetValueByKey("HAS VIEWS").TryConvertStringToBool();
                    string internalId = extracted.GetValueByKey("ID");
                    string lastModified = extracted.GetValueByKey("LAST MODIFIED");
                    bool onSameDomain = extracted.GetValueByKey("ON SAME DOMAIN").TryConvertStringToBool();
                    string packageId = extracted.GetValueByKey("PACKAGE ID");
                    string revisionId = extracted.GetValueByKey("REVISION ID");
                    string state = extracted.GetValueByKey("STATE");
                    string urlType = extracted.GetValueByKey("URL TYPE");

                    DataSourceAditionalInformation aditionalInformation = new(urlFile, lastUpdate, creationDate, format, license,
                                                                              created, hasViews, internalId, lastModified,
                                                                              onSameDomain, packageId, revisionId, state, urlType);

                    aditionalInformationInserted = await _dataSourceAditionalInformation.InsertAsync(aditionalInformation);
                }
            }
            return aditionalInformationInserted;
        }

        private async Task<string> DownloadDataSourceDocument(string url, string search)
        {
            string downloadPath = string.Empty;

            if (!string.IsNullOrEmpty(url))
            {
                downloadPath = url.DownloadFilesFromUrl(Path.Combine(_hostingEnvironment.WebRootPath, "Files"), true);
                await SaveFilesS3(url, search);
            }
            return downloadPath;
        }

        private async Task<DatasetAditionalInformation> ExtractDatasetAddionalInformation(HtmlString html)
        {
            DatasetAditionalInformation aditionalInformationInserted = new();
            Dictionary<string, string> extracted = html.ExtractTableInfo(".//div[@id='content']//table[@class='table table-striped table-bordered table-condensed']");

            if (extracted.Any())
            {
                string source = extracted.GetValueByKey("FONTE");
                string manager = extracted.GetValueByKey("AUTOR");
                DateTime? lastUpdate = extracted.GetValueByKey("ÚLTIMA ATUALIZAÇÃO").TryConvertDatetime("MMMM d, yyyy, HH:mm", "pt-BR");
                DateTime? creationDate = extracted.GetValueByKey("CRIADO").TryConvertDatetime("MMMM d, yyyy, HH:mm", "pt-BR");
                string updateFrequency = extracted.GetValueByKey("PERIODICIDADE DE ATUALIZAÇÃO");

                DatasetAditionalInformation aditionalInformation = new(source, manager, lastUpdate, creationDate, updateFrequency);
                aditionalInformationInserted = await _datasetAditionalInformation.InsertAsync(aditionalInformation);
            }
            return aditionalInformationInserted;
        }

        private async Task<List<UrlExtractedDTO>> GetUrlsBySearchFromMysql(string search)
        {
            List<UrlExtractedDTO> extractUrlsDTO = new();

            if (_parser.GetUrlsMysql)
            {
                var extractUrls = await _extractedRepository.FindAllAsync(x => x.Search.ToUpper().Contains(search));
                extractUrlsDTO = _mapper.Map<List<UrlExtractedDTO>>(extractUrls);
            }
            return extractUrlsDTO;
        }

        private async Task<List<UrlExtractedDTO>> GetUrlsBySearchFromSQS(string search)
        {
            List<UrlExtractedDTO> extractUrlsDTO = new();

            if (_parser.GetUrlsSQS)
            {
                extractUrlsDTO.AddRange(await _SQSHelper.ExtractAndParserListSQSMessages<UrlExtractedDTO>(search, _parser.DeleteAllMessagesSQS, _parser.DeleteQueueSQS));
            }
            return extractUrlsDTO;
        }

        private async Task<List<UrlExtractedDTO>> GetUrlsParser(string search)
        {
            List<UrlExtractedDTO> urlExtracteds = new();
            urlExtracteds.AddRangeIfNotNullOrEmpty(await GetUrlsBySearchFromMysql(search));
            urlExtracteds.AddRangeIfNotNullOrEmpty(await GetUrlsBySearchFromSQS(search));
            return urlExtracteds;
        }

        private async Task<bool> SaveFilesS3(string url, string search)
        {
            if (_parser.S3Save)
            {
                Stream stream = url.DownloadFilesFromUrl();
                string filename = url.GenerateRandomFileName();
                return await _S3Helper.UploadFileAsync(_parser.BucketNameS3, stream, filename, search);
            }
            return false;
        }
    }
}