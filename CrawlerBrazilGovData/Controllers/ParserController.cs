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
    /// API endpoint (Parser)
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class ParserController : ControllerBase
    {
        private readonly IParserService _parserService;
        private readonly ILogger<ParserController> _logger;

        public ParserController(IParserService parserService, ILogger<ParserController> logger)
        {
            _parserService = parserService;
            _logger = logger;
        }

        /// <summary>
        /// Parser all infos extracted in urls by search term
        /// </summary>
        /// <param name="search"> search term </param>
        /// <returns> Array Datasets Parsed </returns>
        [HttpGet("", Name = nameof(ParserBySearch))]
        [SwaggerResponse((int)HttpStatusCode.OK, Description = "Returns 200", Type = typeof(IEnumerable<DatasetDTO>))]
        [SwaggerResponse((int)HttpStatusCode.NotFound, Description = "Missing Datasets objects")]
        [SwaggerResponse((int)HttpStatusCode.InternalServerError, Description = "Unexpected error")]
        public async Task<IActionResult> ParserBySearch([FromQuery] string search)
        {
            _logger.LogDebug($"[GET] - Method: {nameof(ParserBySearch)} - Query: {search}");
            var results = await _parserService.ParserUrlToDataset(search);

            if (results == null)
            {
                _logger.LogTrace($"[RESPONSE] - Method: {nameof(ParserBySearch)} - NOT FOUND - Query: {search}");
                return NotFound("No data was found with the current keyword!");
            }

            _logger.LogTrace($"[RESPONSE] - Method: {nameof(ParserBySearch)} - Count: {results.Count} - Query: {search}");

            return Ok(results);
        }
    }
}