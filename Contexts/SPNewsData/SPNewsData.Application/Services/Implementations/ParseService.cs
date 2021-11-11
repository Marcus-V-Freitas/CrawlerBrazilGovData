using AutoMapper;
using Core.Common;
using Core.Configuration;
using Core.Web.Entities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;
using SPNewsData.Application.Entities.DTOs;
using SPNewsData.Application.Services.Interfaces;
using SPNewsData.Domain.Entities;
using SPNewsData.Domain.Enums;
using SPNewsData.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace SPNewsData.Application.Services.Implementations
{
    public class ParseService : IParserService
    {
        private readonly Configs _configs;
        private readonly HttpClient _client;
        private readonly IMapper _mapper;
        private readonly IUrlExtractedRepository _extractedRepository;
        private readonly IGovNewsRepository _govNewsRepository;
        private readonly IHostingEnvironment _hostingEnvironment;

        private const string _folderSaveEvidences = "Files\\{0}\\html";

        public ParseService(HttpClient client, IMapper mapper,
                            IUrlExtractedRepository extractedRepository, IOptions<Configs> options,
                            IGovNewsRepository govNewsRepository, IHostingEnvironment hostingEnvironment)
        {
            _client = client;
            _mapper = mapper;
            _extractedRepository = extractedRepository;
            _configs = options.Value;
            _govNewsRepository = govNewsRepository;
            _hostingEnvironment = hostingEnvironment;
        }

        public async Task<List<GovNewsDTO>> ParserUrlToGovNews(string search)
        {
            List<GovNews> govNews = new();
            List<UrlExtractedDTO> urls = await GetUrlsParser(search);

            if (!urls.ListIsNullOrEmpty())
            {
                await urls.ParallelForEachAsync(async (url) =>
                {
                    HtmlString html = await _client.GetResponseHtmlAsync(url.Url);
                    if (!html.IsNullOrEmpty)
                    {
                        govNews.AddIfNotNull(await ExtractGeneralInfo(html, url.Url, search));
                    }
                }, _configs.MaxDegreeOfParallelism);
            }
            return await SaveGovNewsInMysql(govNews);
        }

        private async Task<List<GovNewsDTO>> SaveGovNewsInMysql(List<GovNews> govNews)
        {
            List<GovNewsDTO> govNewsDTO = new();
            int page = 1;
            int pagination = 1000;

            while (true)
            {
                var govNewsPagination = govNews.Skip((page - 1) * pagination).Take(pagination).ToList();

                if (govNewsPagination.ListIsNullOrEmpty())
                    break;

                var govNewsInserted = await _govNewsRepository.BulkInsertAsync(govNewsPagination);
                govNewsDTO.AddRangeIfNotNullOrEmpty(_mapper.Map<List<GovNewsDTO>>(govNewsInserted));
            }
            return govNewsDTO;
        }

        private async Task<GovNews> ExtractGeneralInfo(HtmlString html, string url, string search)
        {
            GovNews govNews = ExtractGovNewInformation(html, url, search);
            govNews.Evidences.AddIfNotNull(await SaveEvidences(html, govNews.Id, search));
            govNews.Subjects.AddRangeIfNotNullOrEmpty(ExtractSubjectsInformation(html, govNews.Id));
            return govNews;
        }

        private async Task<Evidence> SaveEvidences(HtmlString html, int? govNewId, string search)
        {
            Evidence evidence = new();
            string fullPath = string.Format(_folderSaveEvidences, search);

            if (await html.SaveHtmlAsync(Path.Combine(_hostingEnvironment.WebRootPath, fullPath)))
            {
                evidence = new(html.FileNameGuid, EvidenceType.HTML, govNewId);
            }
            return evidence;
        }

        private GovNews ExtractGovNewInformation(HtmlString html, string url, string search)
        {
            string title = html.ExtractSingleInfo(".//header[@class='article-header']/h1[@class='title']");
            string subtitle = html.ExtractSingleInfo(".//section/header[@class='article-header']/p");
            string[] infos = html.ExtractSingleInfo(".//section/header//span[@class='date']").Split('|');
            string content = html.ExtractSingleInfo(".//section/article[@class='article-main']");
            DateTime? publicationDate = null;
            string source = null;

            if (infos != null && infos.Length == 2)
            {
                string date = infos[0].TryRemoveTextBeforeValue(" ").Replace("H", ":");
                publicationDate = date.TryConvertDatetime("d/MM/yyyy - H:mm");
                source = infos[1];
            }

            GovNews govNews = new(title, subtitle, publicationDate, source, content, url, search);
            return govNews;
        }

        private List<Subject> ExtractSubjectsInformation(HtmlString html, int? govNewId)
        {
            List<Subject> subjects = new();
            List<string> names = html.ExtractListInfo(".//section/footer/div[@class='categories']/a[@class='category']");

            if (!names.ListIsNullOrEmpty())
            {
                foreach (var name in names)
                {
                    Subject subject = new(name, govNewId);
                    subjects.AddIfNotNull(subject);
                }
            }
            return subjects;
        }

        private async Task<List<UrlExtractedDTO>> GetUrlsBySearchFromMysql(string search)
        {
            List<UrlExtractedDTO> extractUrlsDTO = new();

            if (_configs.SPNewsData.Parser.GetUrlsMysql)
            {
                var extractUrls = await _extractedRepository.FindAllAsync(x => x.Search.ToUpper().Contains(search.ToUpper())
                                                                            && x.ParsingLayout);
                extractUrlsDTO = _mapper.Map<List<UrlExtractedDTO>>(extractUrls);
            }
            return extractUrlsDTO;
        }

        private async Task<List<UrlExtractedDTO>> GetUrlsParser(string search)
        {
            List<UrlExtractedDTO> urlExtracteds = new();
            urlExtracteds.AddRangeIfNotNullOrEmpty(await GetUrlsBySearchFromMysql(search));
            return urlExtracteds;
        }
    }
}