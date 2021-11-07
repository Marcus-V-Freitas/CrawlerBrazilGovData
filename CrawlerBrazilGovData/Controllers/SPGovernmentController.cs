using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SPGovernmentData.Application.Entities.DTOs;
using SPGovernmentData.Application.Services.Interfaces;
using Swashbuckle.AspNetCore.Annotations;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace CrawlerBrazilGovData.Controllers
{
    /// <summary>
    /// API endpoint
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class SPGovernmentController : Controller
    {
        private readonly IExtractUrlsService _extractUrlsService;
        private readonly IParserService _parserService;
        private readonly ILogger<SPGovernmentController> _logger;

        public SPGovernmentController(IExtractUrlsService extractUrlsService, ILogger<SPGovernmentController> logger, IParserService parserService)
        {
            _extractUrlsService = extractUrlsService;
            _logger = logger;
            _parserService = parserService;
        }

        /// <summary>
        /// Extract all url by search term
        /// </summary>
        /// <param name="search"> search term </param>
        /// <returns> Array Urls Found </returns>
        [HttpPost("Bootstrap", Name = nameof(GovExtractUrlsBySearch))]
        [SwaggerResponse((int)HttpStatusCode.OK, Description = "Returns 200", Type = typeof(IEnumerable<UrlExtractedDTO>))]
        [SwaggerResponse((int)HttpStatusCode.NotFound, Description = "Missing Urls objects")]
        [SwaggerResponse((int)HttpStatusCode.InternalServerError, Description = "Unexpected error")]
        public async Task<IActionResult> GovExtractUrlsBySearch([FromBody] string search)
        {
            _logger.LogDebug($"[GET] - Method: {nameof(GovExtractUrlsBySearch)} - Query: {search}");

            var results = await _extractUrlsService.ExtractUrlsBySearch(search);

            if (results == null)
            {
                _logger.LogTrace($"[RESPONSE] - Method: {nameof(GovExtractUrlsBySearch)} - NOT FOUND - Query: {search}");
                return NotFound("No data was found with the current keyword!");
            }

            _logger.LogTrace($"[RESPONSE] - Method: {nameof(GovExtractUrlsBySearch)} - Count: {results.Count} - Query: {search}");

            return Ok(results);
        }

        /// <summary>
        /// Parser all infos extracted in urls by search term
        /// </summary>
        /// <param name="search"> search term </param>
        /// <returns> Array Datasets Parsed </returns>
        [HttpPost("Parser", Name = nameof(GovParserBySearch))]
        [SwaggerResponse((int)HttpStatusCode.OK, Description = "Returns 200", Type = typeof(IEnumerable<DatasetDTO>))]
        [SwaggerResponse((int)HttpStatusCode.NotFound, Description = "Missing Datasets objects")]
        [SwaggerResponse((int)HttpStatusCode.InternalServerError, Description = "Unexpected error")]
        public async Task<IActionResult> GovParserBySearch([FromBody] string search)
        {
            _logger.LogDebug($"[GET] - Method: {nameof(GovParserBySearch)} - Query: {search}");
            var results = await _parserService.ParserUrlToDataset(search);

            if (results == null)
            {
                _logger.LogTrace($"[RESPONSE] - Method: {nameof(GovParserBySearch)} - NOT FOUND - Query: {search}");
                return NotFound("No data was found with the current keyword!");
            }

            _logger.LogTrace($"[RESPONSE] - Method: {nameof(GovParserBySearch)} - Count: {results.Count} - Query: {search}");

            return Ok(results);
        }
    }
}