using Application.Entities.DTOs;
using Application.Services.Interfaces;
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
        private readonly IParserService _parserService;

        public ParserController(IParserService parserService)
        {
            _parserService = parserService;
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
            var datasets = await _parserService.ParserUrlToDataset(search);

            if (datasets == null)
            {
                return NotFound("No data was found with the current keyword!");
            }
            return Ok(datasets);
        }
    }
}