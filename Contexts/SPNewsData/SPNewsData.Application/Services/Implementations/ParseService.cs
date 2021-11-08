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
        private readonly Parser _parser;
        private readonly HttpClient _client;
        private readonly IMapper _mapper;
        private readonly IUrlExtractedRepository _extractedRepository;
        private readonly IGovNewsRepository _govNewsRepository;
        private readonly ISubjectRepository _subjectRepository;
        private readonly IEvidenceRepository _evidenceRepository;
        private readonly IHostingEnvironment _hostingEnvironment;

        private const string _folderSaveEvidences = "Files\\{0}\\html";

        public ParseService(HttpClient client, IMapper mapper,
                            IUrlExtractedRepository extractedRepository, IOptions<Configs> options,
                             IGovNewsRepository govNewsRepository, ISubjectRepository subjectRepository, IEvidenceRepository evidenceRepository, IHostingEnvironment hostingEnvironment)
        {
            _client = client;
            _mapper = mapper;
            _extractedRepository = extractedRepository;
            _parser = options.Value.SPNewsData.Parser;
            _govNewsRepository = govNewsRepository;
            _subjectRepository = subjectRepository;
            _evidenceRepository = evidenceRepository;
            _hostingEnvironment = hostingEnvironment;
        }

        public async Task<List<GovNewsDTO>> ParserUrlToGovNews(string search)
        {
            List<GovNewsDTO> govNewsDTOs = new();
            List<UrlExtractedDTO> urls = await GetUrlsParser(search);

            if (!urls.ListIsNullOrEmpty())
            {
                foreach (UrlExtractedDTO url in urls)
                {
                    HtmlString html = await _client.GetResponseHtmlAsync(url.Url);
                    if (!html.IsNullOrEmpty)
                    {
                        govNewsDTOs.AddIfNotNull(await ExtractGeneralInfo(html, url.Url, search));
                    }
                }
            }
            return govNewsDTOs;
        }

        private async Task<GovNewsDTO> ExtractGeneralInfo(HtmlString html, string url, string search)
        {
            GovNewsDTO govNews = await ExtractGovNewInformation(html, url, search);
            govNews.Evidences.AddIfNotNull(await SaveEvidences(html, govNews.Id, search));
            govNews.Subjects.AddRangeIfNotNullOrEmpty(await ExtractSubjectsInformation(html, govNews.Id));
            return govNews;
        }

        private async Task<EvidenceDTO> SaveEvidences(HtmlString html, int? govNewId, string search)
        {
            EvidenceDTO evidenceDto = new();
            string fullPath = string.Format(_folderSaveEvidences, search);

            if (html.SaveHtml(Path.Combine(_hostingEnvironment.WebRootPath, fullPath)))
            {
                Evidence evidence = new(html.FileNameGuid, EvidenceType.HTML, govNewId);
                var evidenceInserted = await _evidenceRepository.InsertAsync(evidence);
                evidenceDto = _mapper.Map<EvidenceDTO>(evidenceInserted);
            }
            return evidenceDto;
        }

        private async Task<GovNewsDTO> ExtractGovNewInformation(HtmlString html, string url, string search)
        {
            string title = html.ExtractSingleInfo(".//header[@class='article-header']/h1[@class='title']");
            string subtitle = html.ExtractSingleInfo(".//section/header[@class='article-header']/p");
            string[] infos = html.ExtractSingleInfo(".//section/header//span[@class='date']").Split('|');
            string content = html.ExtractSingleInfo(".//section/article[@class='article-main']");
            DateTime? publicationDate = null;
            string source = null;

            if (infos != null && infos.Count() == 2)
            {
                string date = infos[0].TryRemoveTextBeforeValue(" ").Replace("H", ":");
                publicationDate = date.TryConvertDatetime("d/MM/yyyy - H:mm");
                source = infos[1];
            }

            GovNews govNews = new(title, subtitle, publicationDate, source, content, url, search);
            var govNewInserted = await _govNewsRepository.InsertAsync(govNews);
            GovNewsDTO govNewsDTO = _mapper.Map<GovNewsDTO>(govNewInserted);

            return govNewsDTO;
        }

        private async Task<List<SubjectDTO>> ExtractSubjectsInformation(HtmlString html, int? govNewId)
        {
            List<SubjectDTO> subjectDTOs = new();
            List<string> names = html.ExtractListInfo(".//section/footer/div[@class='categories']/a[@class='category']");

            if (!names.ListIsNullOrEmpty())
            {
                foreach (var name in names)
                {
                    Subject subject = new(name, govNewId);
                    var subjectInserted = await _subjectRepository.InsertAsync(subject);
                    subjectDTOs.AddIfNotNull(_mapper.Map<SubjectDTO>(subjectInserted));
                }
            }
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