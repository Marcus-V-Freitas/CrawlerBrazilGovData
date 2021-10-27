using Application.Common;
using Application.Services.Interfaces;
using Core.Utils;
using Domain.Entities;
using Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Application.Services.Implementations
{
    public class ParserService : GovDataUtils, IParserService
    {
        private readonly HttpClient _client;
        private readonly IDatasetRepository _datasetRepository;
        private readonly IDataSourceRepository _dataSourceRepository;
        private readonly ITagRepository _tagRepository;
        private readonly IDatasetAditionalInformationRepository _datasetAditionalInformation;
        private readonly IDataSourceAditionalInformationRepository _dataSourceAditionalInformation;

        public ParserService(IDatasetRepository datasetRepository, HttpClient client, IDatasetAditionalInformationRepository datasetAditionalInformation, ITagRepository tagRepository, IDataSourceAditionalInformationRepository dataSourceAditionalInformation, IDataSourceRepository dataSourceRepository)
        {
            _client = client;
            _datasetRepository = datasetRepository;
            _datasetAditionalInformation = datasetAditionalInformation;
            _tagRepository = tagRepository;
            _dataSourceAditionalInformation = dataSourceAditionalInformation;
            _dataSourceRepository = dataSourceRepository;
        }

        public async Task<List<Dataset>> ParserUrlToDataset(List<UrlExtracted> urls)
        {
            List<Dataset> datasets = new();

            if (urls != null && urls.Any())
            {
                foreach (UrlExtracted url in urls)
                {
                    string html = await _client.GetResponseHtmlAsync(url.Url);
                    if (!string.IsNullOrEmpty(html))
                    {
                        Dataset dataset = await ExtractGeneralInfo(html);
                        if (dataset != null)
                        {
                            datasets.Add(dataset);
                        }
                    }
                }
            }
            return datasets;
        }

        private async Task<Dataset> ExtractGeneralInfo(string html)
        {
            DatasetAditionalInformation aditionalInformation = await ExtractDatasetAddionalInformation(html);
            Dataset dataset = await ExtractDatasetInformation(html, aditionalInformation.Id);
            await ExtractTagInformation(html, dataset.Id);
            await ExtractDataSourcesInformation(html, dataset);
            return dataset;
        }

        private async Task<List<DataSource>> ExtractDataSourcesInformation(string html, Dataset dataset)
        {
            List<string> titles = html.ExtractListInfo(".//div[@id='content']//article[@class='module']//ul[@class='resource-list']/li[@class='resource-item']//a[@class='heading']");
            List<string> urls = html.ExtractListInfoAttributes(".//div[@id='content']//article[@class='module']//ul[@class='resource-list']/li[@class='resource-item']//a[@class='heading']", "href", false);
            List<DataSource> dataSources = new();

            if (titles !=null && urls !=null && titles.Count() == urls.Count())
            {
                foreach (int indice in Enumerable.Range(0, titles.Count))
                {
                    string url = _baseUrlGov.CombineUrl(urls[indice]);
                    var addionalInformation = await ExtractDatasourceAddionalInformation(url);

                    if (addionalInformation != null)
                    {
                        DataSource dataSource = new(titles[indice], url, dataset.Title, addionalInformation.Id, dataset.Id);
                        var dataSourceInserted = await _dataSourceRepository.InsertAsync(dataSource);
                        dataSources.Add(dataSourceInserted);
                    }
                }
            }
            return dataSources;
        }

        private async Task<List<Tag>> ExtractTagInformation(string html, int? datasetId)
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

        private async Task<Dataset> ExtractDatasetInformation(string html, int? aditionalInformationId)
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
            string html = await _client.GetResponseHtmlAsync(url);

            if (!string.IsNullOrEmpty(html))
            {
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

                    DataSourceAditionalInformation aditionalInformation = new(lastUpdate, creationDate, format, license,
                                                                              created, hasViews, internalId, lastModified,
                                                                              onSameDomain, packageId, revisionId, state, urlType);

                    aditionalInformationInserted = await _dataSourceAditionalInformation.InsertAsync(aditionalInformation);
                }
            }
            return aditionalInformationInserted;
        }

        private async Task<DatasetAditionalInformation> ExtractDatasetAddionalInformation(string html)
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
    }
}