using Application.Services.Interfaces;
using Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace CrawlerBrazilGovData.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class ParserController : ControllerBase
    {
        private readonly IExtractUrlsService _extractUrlsService;
        private readonly IParserService _parserService;

        public ParserController(IExtractUrlsService extractUrlsService, IParserService parserService)
        {
            _extractUrlsService = extractUrlsService;
            _parserService = parserService;
        }

        /// <summary>
        /// Parser all infos extracted in urls by search term
        /// </summary>
        /// <param name="search"> search term </param>
        /// <returns> Array Datasets Parsed </returns>
        [HttpGet("", Name = nameof(ParserBySearch))]
        [SwaggerResponse((int)HttpStatusCode.OK, Description = "Returns 200", Type = typeof(IEnumerable<Dataset>))]
        [SwaggerResponse((int)HttpStatusCode.NotFound, Description = "Missing Datasets objects")]
        [SwaggerResponse((int)HttpStatusCode.InternalServerError, Description = "Unexpected error")]
        public async Task<IActionResult> ParserBySearch([FromQuery] string search)
        {
            var urls = await _extractUrlsService.GetUrlsBySearch(search);

            if (urls == null)
            {
                return NotFound("No data was found with the current keyword!");
            }
            return Ok(await _parserService.ParserUrlToDataset(urls));
        }
    }
}