using Application.Common;
using Application.Entities.Configuration;
using Application.Entities.DTOs;
using Application.Services.Interfaces;
using AutoMapper;
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
        private readonly Configs _configs;
        private readonly HttpClient _client;
        private readonly IMapper _mapper;
        private readonly ISQSHelper _SQSHelper;
        private readonly IUrlExtractedRepository _urlExtractedRepository;

        public ExtractUrlsService(IUrlExtractedRepository urlExtractedRepository, HttpClient client, ISQSHelper sQSHelper, IOptions<Configs> options, IMapper mapper)
        {
            _urlExtractedRepository = urlExtractedRepository;
            _client = client;
            _SQSHelper = sQSHelper;
            _configs = options.Value;
            _mapper = mapper;
        }

        public async Task<List<UrlExtractedDTO>> ExtractUrlsBySearch(string search)
        {
            List<UrlExtractedDTO> extratedUrlsDTO = new();
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
                    var extratedUrls = _mapper.Map<UrlExtractedDTO>(urlExtractedInserted);
                    extratedUrlsDTO.Add(extratedUrls);
                }
            }

            await SaveSQS(search, extratedUrlsDTO);

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

        private async Task<string> SaveSQS(string search, List<UrlExtractedDTO> extratedUrls)
        {
            string messageId = string.Empty;

            if (_configs.Bootstrap.SQSSave)
            {
                var queue = await _SQSHelper.CreateQueue(search);

                if (queue.HttpStatusCode == HttpStatusCode.OK)
                {
                    string json = JsonConvert.SerializeObject(extratedUrls);
                    var sendMessage = await _SQSHelper.SendMessage(search, json);

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
            UrlExtracted urlExtractedInserted = new();
            if (_configs.Bootstrap.MysqlSave)
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