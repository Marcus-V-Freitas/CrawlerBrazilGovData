using AutoMapper;
using Core.Common;
using Core.Configuration;
using Core.Utils;
using Core.Web.Entities;
using HtmlAgilityPack;
using Microsoft.Extensions.Options;
using SPNewsData.Application.Entities.DTOs;
using SPNewsData.Application.Services.Interfaces;
using SPNewsData.Domain.Entities;
using SPNewsData.Domain.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace SPNewsData.Application.Services.Implementations
{
    public class ExtractBatchUrlsService : GovDataUtils, IExtractBatchUrlsService
    {
        private readonly Bootstrap _bootstrap;
        private readonly HttpClient _client;
        private readonly IMapper _mapper;
        private readonly IUrlExtractedRepository _urlExtractedRepository;

        private const string _search = "TODOS";

        public ExtractBatchUrlsService(IUrlExtractedRepository urlExtractedRepository,
                                        HttpClient client, IOptions<Configs> options, IMapper mapper)
        {
            _urlExtractedRepository = urlExtractedRepository;
            _client = client;
            _bootstrap = options.Value.SPNewsData.Bootstrap;
            _mapper = mapper;
        }

        protected override string _baseUrlGov => "https://www.saopaulo.sp.gov.br";
        protected override string _searchQueryString => "/page/{0}/?s&ordenar=date&limite=1000";

        public async Task<List<UrlExtractedDTO>> ExtractUrlsBySearch()
        {
            List<UrlExtracted> extratedUrls = new();

            int counter = 1;
            while (true)
            {
                HtmlString htmlResponse = await _client.GetResponseHtmlAsync(string.Format(_baseUrlGov.CombineUrl(_searchQueryString), counter));

                if (string.IsNullOrEmpty(htmlResponse.ToHtmlString) || htmlResponse.ToHtmlString.ContainsInsensitive("DESCULPE, MAS A PÁGINA SOLICITADA"))
                    break;

                var currentPageUrls = GetUrlsByPageNumber(htmlResponse);
                extratedUrls.AddRangeIfNotNullOrEmpty(await SaveMysql(currentPageUrls));
                counter++;
            }

            return _mapper.Map<List<UrlExtractedDTO>>(extratedUrls);
        }

        private List<UrlExtracted> GetUrlsByPageNumber(HtmlString htmlResponse)
        {
            List<UrlExtracted> extratedUrls = new();
            HtmlNodeCollection urlNodes = htmlResponse.ExtractListNodes(".//div[@class='content']//article//h3/a");

            if (urlNodes == null || !urlNodes.Any())
            {
                return null;
            }

            foreach (var urlNode in urlNodes)
            {
                string title = urlNode.InnerText;
                string url = urlNode.Attributes["href"].Value;
                bool parsingLayout = !string.IsNullOrEmpty(title);

                var urlExtracted = new UrlExtracted(title, url, _search, parsingLayout);
                extratedUrls.AddIfNotNull(urlExtracted);
            }

            return extratedUrls;
        }

        private async Task<List<UrlExtracted>> SaveMysql(List<UrlExtracted> urlsExtracted)
        {
            List<UrlExtracted> urlsExtractedInserted = new();

            if (_bootstrap.MysqlSave)
            {
                urlsExtractedInserted.AddRangeIfNotNullOrEmpty(await _urlExtractedRepository.BulkInsertAsync(urlsExtracted));
            }
            else
            {
                urlsExtractedInserted = urlsExtracted;
            }
            return urlsExtractedInserted;
        }

        public async Task<List<UrlExtractedDTO>> GetUrlsBySearch()
        {
            List<UrlExtractedDTO> urlExtractedDTOs = new();

            var extractedUrl = await _urlExtractedRepository.FindAllAsync(x => x.Search == _search);

            if (extractedUrl != null)
            {
                urlExtractedDTOs = _mapper.Map<List<UrlExtractedDTO>>(extractedUrl);
            }
            return urlExtractedDTOs;
        }
    }
}