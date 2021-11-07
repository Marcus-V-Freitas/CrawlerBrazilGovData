using AutoMapper;
using Core.Common;
using Core.Configuration;
using Core.Web.Entities;
using Microsoft.Extensions.Options;
using SPNewsData.Application.Entities.DTOs;
using SPNewsData.Application.Services.Interfaces;
using SPNewsData.Domain.Entities;
using SPNewsData.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace SPNewsData.Application.Services.Implementations
{
    public class ParseService : IParserService
    {
        private readonly Parser _parser;
        private readonly HttpClient _client;
        private readonly IMapper _mapper;
        private readonly IUrlExtractedRepository _extractedRepository;
        private readonly IGovNewsRepository _govNewsRepository;

        public ParseService(HttpClient client, IMapper mapper,
                            IUrlExtractedRepository extractedRepository,
                            IOptions<Configs> options, IGovNewsRepository govNewsRepository)
        {
            _client = client;
            _mapper = mapper;
            _extractedRepository = extractedRepository;
            _parser = options.Value.SPNewsData.Parser;
            _govNewsRepository = govNewsRepository;
        }

        public async Task<List<GovNewsDTO>> ParserUrlToGovNews(string search)
        {
            List<GovNewsDTO> govNewsDTOs = new();
            List<UrlExtractedDTO> urls = await GetUrlsParser(search);

            if (urls != null && urls.Any())
            {
                foreach (UrlExtractedDTO url in urls)
                {
                    HtmlString html = await _client.GetResponseHtmlAsync(url.Url);
                    if (!html.IsNullOrEmpty)
                    {
                        govNewsDTOs.Add(await ExtractGeneralInfo(html));
                    }
                }
            }

            return govNewsDTOs;
        }

        private async Task<GovNewsDTO> ExtractGeneralInfo(HtmlString html)
        {
            GovNewsDTO govNews = await ExtractGovNewInformation(html);
            //await ExtractSubjectsInformation(html, govNews.Id);
            return govNews;
        }

        private async Task<GovNewsDTO> ExtractGovNewInformation(HtmlString html)
        {
            string title = html.ExtractSingleInfo(".//header[@class='article-header']/h1[@class='title']");
            string subtitle = html.ExtractSingleInfo(".//section/header[@class='article-header']/p");
            string[] infos = html.ExtractSingleInfo(".//section/header/div[@class='meta']/span[@class='date']").Split('|');
            string content = html.ExtractSingleInfo(".//section/article[@class='article-main']");
            DateTime? publicationDate = null;
            string source = null;

            if (infos != null && infos.Count() == 2)
            {
                string date = infos[0].TryRemoveTextBeforeValue(" ").Replace("H", "");
                publicationDate = date.TryConvertDatetime("d/MM/yyyy - HHmm");
                source = infos[1];
            }

            GovNews govNews = new(title, subtitle, publicationDate, source, content);
            var govNewInserted = await _govNewsRepository.InsertAsync(govNews);
            GovNewsDTO govNewsDTO = _mapper.Map<GovNewsDTO>(govNewInserted);

            return govNewsDTO;
        }

        private async Task<List<SubjectDTO>> ExtractSubjectsInformation(HtmlString html, int? govNewId)
        {
            List<SubjectDTO> subjectDTOs = new();

            return subjectDTOs;
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

        private async Task<List<UrlExtractedDTO>> GetUrlsParser(string search)
        {
            List<UrlExtractedDTO> urlExtracteds = new();
            urlExtracteds.AddRangeIfNotNullOrEmpty(await GetUrlsBySearchFromMysql(search));
            return urlExtracteds;
        }
    }
}