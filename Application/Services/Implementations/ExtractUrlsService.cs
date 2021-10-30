using Application.Common;
using Application.Entities.Configuration;
using Application.Services.Interfaces;
using AWSHelpers.SQS.Interfaces;
using Core.Utils;
using Domain.Entities;
using Domain.Interfaces;
using HtmlAgilityPack;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Application.Services.Implementations
{
    public class ExtractUrlsService : GovDataUtils, IExtractUrlsService
    {
        private readonly HttpClient _client;
        private readonly ISQSHelper _SQSHelper;
        private readonly Configs _configs;
        private readonly IUrlExtractedRepository _urlExtractedRepository;

        public ExtractUrlsService(IUrlExtractedRepository urlExtractedRepository, HttpClient client, ISQSHelper sQSHelper, IOptions<Configs> options)
        {
            _urlExtractedRepository = urlExtractedRepository;
            _client = client;
            _SQSHelper = sQSHelper;
            _configs = options.Value;
        }

        public async Task<List<UrlExtracted>> ExtractUrlsBySearch(string search)
        {
            List<UrlExtracted> extratedUrls = new();
            var htmlResponse = await _client.GetResponseHtmlAsync(string.Format(_baseUrlGov.CombineUrl(_searchDatasets), search));
            HtmlDocument doc = htmlResponse.CreateHtmlDocument();
            HtmlNodeCollection urlNodes = doc.DocumentNode.SelectNodes(".//div[@id='content']//li[@class='dataset-item']//h3[@class='dataset-heading']/a");

            if (urlNodes == null || !urlNodes.Any())
            {
                return null;
            }

            foreach (var urlNode in urlNodes)
            {
                var urlExtracted = new UrlExtracted(urlNode.InnerText, _baseUrlGov.CombineUrl(urlNode.Attributes["href"].Value), search);
                var urlExtractedInserted = await SaveMysql(search, urlExtracted);

                if (urlExtractedInserted != null)
                {
                    extratedUrls.Add(urlExtractedInserted);
                }
            }

            await SaveSQS(search, extratedUrls);

            return extratedUrls;
        }

        public async Task<List<UrlExtracted>> GetUrlsBySearch(string search)
        {
            return await _urlExtractedRepository.FindAllAsync(x => x.Search == search);
        }

        private async Task<string> SaveSQS(string search, List<UrlExtracted> extratedUrls)
        {
            string messageId = string.Empty;

            if (_configs.Bootstrap.SQSSave)
            {
                var queue = await _SQSHelper.CreateQueue(search);

                if (queue.HttpStatusCode == HttpStatusCode.OK)
                {
                    string json = JsonConvert.SerializeObject(extratedUrls);
                    var sendMessage = await _SQSHelper.SendMessage(queue.QueueUrl, json);

                    if (sendMessage.HttpStatusCode == HttpStatusCode.OK)
                    {
                        messageId = sendMessage.MessageId;
                    }
                }
            }
            return messageId;
        }

        private async Task<UrlExtracted> SaveMysql(string search, UrlExtracted urlExtracted)
        {
            var urlExtractedInserted = urlExtracted;
            if (_configs.Bootstrap.MysqlSave)
            {
                await _urlExtractedRepository.FindAsync(x => x.Search == search && x.Title == urlExtracted.Title);

                if (urlExtractedInserted == null)
                {
                    urlExtractedInserted = await _urlExtractedRepository.InsertAsync(urlExtracted);
                }
            }
            return urlExtractedInserted;
        }
    }
}