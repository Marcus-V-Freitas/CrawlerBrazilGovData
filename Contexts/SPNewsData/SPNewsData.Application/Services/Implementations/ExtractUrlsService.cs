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
    public class ExtractUrlsService : GovDataUtils, IExtractUrlsService
    {
        private readonly Bootstrap _bootstrap;
        private readonly HttpClient _client;
        private int _pageNumbers;
        private readonly IMapper _mapper;
        private readonly IUrlExtractedRepository _urlExtractedRepository;

        public ExtractUrlsService(IUrlExtractedRepository urlExtractedRepository, HttpClient client, IOptions<Configs> options, IMapper mapper)
        {
            _urlExtractedRepository = urlExtractedRepository;
            _client = client;
            _bootstrap = options.Value.SPNewsData.Bootstrap;
            _mapper = mapper;
        }

        protected override string _baseUrlGov => "https://www.saopaulo.sp.gov.br";
        protected override string _searchQueryString => "/page/{0}/?s={1}";

        public async Task<List<UrlExtractedDTO>> ExtractUrlsBySearch(string search)
        {
            List<UrlExtractedDTO> extratedUrlsDTO = new();
            extratedUrlsDTO.AddRangeIfNotNullOrEmpty(await GetUrlsByPageNumber(search, 1, true));

            if (_pageNumbers > 1)
            {
                foreach (int pageNumber in Enumerable.Range(2, _pageNumbers))
                {
                    extratedUrlsDTO.AddRangeIfNotNullOrEmpty(await GetUrlsByPageNumber(search, pageNumber));
                }
            }

            return extratedUrlsDTO;
        }

        public async Task<List<UrlExtractedDTO>> GetUrlsBySearch(string search)
        {
            List<UrlExtractedDTO> urlExtractedDTOs = new();

            var extractedUrl = await _urlExtractedRepository.FindAllAsync(x => x.Search == search);

            if (extractedUrl != null)
            {
                urlExtractedDTOs = _mapper.Map<List<UrlExtractedDTO>>(extractedUrl);
            }
            return urlExtractedDTOs;
        }

        private async Task<List<UrlExtractedDTO>> GetUrlsByPageNumber(string search, int pageNumber, bool extractPageTotal = false)
        {
            List<UrlExtractedDTO> extratedUrlsDTO = new();
            HtmlString htmlResponse = await _client.GetResponseHtmlAsync(string.Format(_baseUrlGov.CombineUrl(_searchQueryString), pageNumber, search));
            HtmlNodeCollection urlNodes = htmlResponse.ExtractListNodes(".//div[@class='content']//article//h3/a");

            if (extractPageTotal)
            {
                _pageNumbers = htmlResponse.ExtractSingleInfo(".//main//div[@class='category-pagination']/div[@class='pagination']/a[@class='page-numbers'][last()]").TryConvertStringToInt();
            }

            if (urlNodes == null || !urlNodes.Any() || _pageNumbers == 0)
            {
                return null;
            }

            foreach (var urlNode in urlNodes)
            {
                string title = urlNode.InnerText;
                string url = urlNode.Attributes["href"].Value;
                bool parsingLayout = !string.IsNullOrEmpty(title);

                var urlExtracted = new UrlExtracted(title, url, search, parsingLayout);
                var urlExtractedInserted = await SaveMysql(search, urlExtracted);

                if (urlExtractedInserted != null)
                {
                    var extratedUrls = _mapper.Map<UrlExtractedDTO>(urlExtractedInserted);
                    extratedUrlsDTO.AddIfNotNull(extratedUrls);
                }
            }

            return extratedUrlsDTO;
        }

        private async Task<UrlExtracted> SaveMysql(string search, UrlExtracted urlExtracted)
        {
            UrlExtracted urlExtractedInserted = new();
            if (_bootstrap.MysqlSave)
            {
                var urlExtractedPreviousInserted = await _urlExtractedRepository.FindAsync(x => x.Search == search && x.Title == urlExtracted.Title);

                if (urlExtractedPreviousInserted == null)
                {
                    urlExtractedInserted = await _urlExtractedRepository.InsertAsync(urlExtracted);
                }
            }
            else
            {
                urlExtractedInserted = urlExtracted;
            }
            return urlExtractedInserted;
        }
    }
}