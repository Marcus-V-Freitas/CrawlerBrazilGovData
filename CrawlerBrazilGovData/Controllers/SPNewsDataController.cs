using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SPNewsData.Application.Services.Interfaces;
using Swashbuckle.AspNetCore.Annotations;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace CrawlerBrazilGovData.Controllers
{
    /// <summary>
    /// API endpoint
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class SPNewsDataController : ControllerBase
    {
        private readonly IExtractUrlsService _extractUrlsService;
        private readonly ILogger<SPNewsDataController> _logger;

        public SPNewsDataController(IExtractUrlsService extractUrlsService, ILogger<SPNewsDataController> logger)
        {
            _extractUrlsService = extractUrlsService;
            _logger = logger;
        }

        /// <summary>
        /// Extract all url by search term
        /// </summary>
        /// <param name="search"> search term </param>
        /// <returns> Array Urls Found </returns>
        [HttpPost("Bootstrap", Name = nameof(NewsExtractUrlsBySearch))]
        [SwaggerResponse((int)HttpStatusCode.OK, Description = "Returns 200", Type = typeof(IEnumerable<SPNewsData.Application.Entities.DTOs.UrlExtractedDTO>))]
        [SwaggerResponse((int)HttpStatusCode.NotFound, Description = "Missing Urls objects")]
        [SwaggerResponse((int)HttpStatusCode.InternalServerError, Description = "Unexpected error")]
        public async Task<IActionResult> NewsExtractUrlsBySearch([FromBody] string search)
        {
            _logger.LogDebug($"[GET] - Method: {nameof(NewsExtractUrlsBySearch)} - Query: {search}");

            var results = await _extractUrlsService.ExtractUrlsBySearch(search);

            if (results == null)
            {
                _logger.LogTrace($"[RESPONSE] - Method: {nameof(NewsExtractUrlsBySearch)} - NOT FOUND - Query: {search}");
                return NotFound("No data was found with the current keyword!");
            }

            _logger.LogTrace($"[RESPONSE] - Method: {nameof(NewsExtractUrlsBySearch)} - Count: {results.Count} - Query: {search}");

            return Ok(results);
        }
    }
}