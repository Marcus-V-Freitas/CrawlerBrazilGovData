using AutoMapper;
using AWSHelpers.SQS.Interfaces;
using Core.Common;
using Core.Configuration;
using Core.Utils;
using Core.Web.Entities;
using HtmlAgilityPack;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using SPGovernmentData.Application.Entities.DTOs;
using SPGovernmentData.Application.Services.Interfaces;
using SPGovernmentData.Domain.Entities;
using SPGovernmentData.Domain.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace SPGovernmentData.Application.Services.Implementations
{
    public class ExtractUrlsService : GovDataUtils, IExtractUrlsService
    {
        private readonly Bootstrap _bootstrap;
        private readonly HttpClient _client;
        private readonly IMapper _mapper;
        private readonly ISQSHelper _SQSHelper;
        private readonly IUrlExtractedRepository _urlExtractedRepository;

        public ExtractUrlsService(IUrlExtractedRepository urlExtractedRepository, HttpClient client, ISQSHelper sQSHelper, IOptions<Configs> options, IMapper mapper)
        {
            _urlExtractedRepository = urlExtractedRepository;
            _client = client;
            _SQSHelper = sQSHelper;
            _bootstrap = options.Value.SPGovernmentData.Bootstrap;
            _mapper = mapper;
        }

        protected override string _baseUrlGov => "http://dados.prefeitura.sp.gov.br";

        protected override string _searchQueryString => "pt_PT/dataset?q={0}&sort=score+desc%2C+metadata_modified+desc";

        public async Task<List<UrlExtractedDTO>> ExtractUrlsBySearch(string search)
        {
            List<UrlExtractedDTO> extratedUrlsDTO = new();
            HtmlString htmlResponse = await _client.GetResponseHtmlAsync(string.Format(_baseUrlGov.CombineUrl(_searchQueryString), search));
            HtmlNodeCollection urlNodes = htmlResponse.ExtractListNodes(".//div[@id='content']//li[@class='dataset-item']//h3[@class='dataset-heading']/a");

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

            if (_bootstrap.SQSSave)
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