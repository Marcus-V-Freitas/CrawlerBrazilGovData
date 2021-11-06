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
    /// API endpoint (ExtractUrls)
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class ExtractUrlsController : Controller
    {
        private readonly IExtractUrlsService _extractUrlsService;
        private readonly ILogger<ExtractUrlsController> _logger;

        public ExtractUrlsController(IExtractUrlsService extractUrlsService, ILogger<ExtractUrlsController> logger)
        {
            _extractUrlsService = extractUrlsService;
            _logger = logger;
        }

        /// <summary>
        /// Extract all url by search term
        /// </summary>
        /// <param name="search"> search term </param>
        /// <returns> Array Urls Found </returns>
        [HttpGet("", Name = nameof(ExtractUrlsBySearch))]
        [SwaggerResponse((int)HttpStatusCode.OK, Description = "Returns 200", Type = typeof(IEnumerable<UrlExtractedDTO>))]
        [SwaggerResponse((int)HttpStatusCode.NotFound, Description = "Missing Urls objects")]
        [SwaggerResponse((int)HttpStatusCode.InternalServerError, Description = "Unexpected error")]
        public async Task<IActionResult> ExtractUrlsBySearch([FromQuery] string search)
        {
            _logger.LogDebug($"[GET] - Method: {nameof(ExtractUrlsBySearch)} - Query: {search}");

            var results = await _extractUrlsService.ExtractUrlsBySearch(search);

            if (results == null)
            {
                _logger.LogTrace($"[RESPONSE] - Method: {nameof(ExtractUrlsBySearch)} - NOT FOUND - Query: {search}");
                return NotFound("No data was found with the current keyword!");
            }

            _logger.LogTrace($"[RESPONSE] - Method: {nameof(ExtractUrlsBySearch)} - Count: {results.Count} - Query: {search}");

            return Ok(results);
        }
    }
}