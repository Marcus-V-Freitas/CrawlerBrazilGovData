using Application.Common;
using Application.Services.Interfaces;
using Core.Utils;
using Domain.Entities;
using Domain.Interfaces;
using HtmlAgilityPack;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Application.Services.Implementations
{
    public class ExtractUrlsService : GovDataUtils, IExtractUrlsService
    {
        private readonly HttpClient _client;
        private readonly IUrlExtractedRepository _urlExtractedRepository;

        public ExtractUrlsService(IUrlExtractedRepository urlExtractedRepository, HttpClient client)
        {
            _urlExtractedRepository = urlExtractedRepository;
            _client = client;
        }

        public async Task<List<UrlExtracted>> ExtractUrlsBySearch(string search)
        {
            List<UrlExtracted> extratedUrls = new();

            var htmlResponse = await _client.GetResponseHtmlAsync(string.Format(_baseUrlGov.CombineUrl(_searchDatasets), search));
            HtmlDocument doc = htmlResponse.CreateHtmlDocument();
            HtmlNodeCollection urlNodes = doc.DocumentNode.SelectNodes(".//div[@id='content']//li[@class='dataset-item']//h3[@class='dataset-heading']/a");

            if (urlNodes == null || !urlNodes.Any())
                return null;

            foreach (var urlNode in urlNodes)
            {
                var urlExtracted = new UrlExtracted(urlNode.InnerText, _baseUrlGov.CombineUrl(urlNode.Attributes["href"].Value), search);
                var urlExtractedInserted = await _urlExtractedRepository.FindAsync(x => x.Search == search &&
                                                                                        x.Title == urlExtracted.Title);

                if (urlExtractedInserted == null)
                {
                    urlExtractedInserted = await _urlExtractedRepository.InsertAsync(urlExtracted);
                }

                if (urlExtractedInserted != null)
                {
                    extratedUrls.Add(urlExtractedInserted);
                }
            }
            return extratedUrls;
        }

        public async Task<List<UrlExtracted>> GetUrlsBySearch(string search)
        {
            return await _urlExtractedRepository.FindAllAsync(x => x.Search == search);
        }
    }
}