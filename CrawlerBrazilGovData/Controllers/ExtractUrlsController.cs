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
    public class ExtractUrlsController : Controller
    {
        private readonly IExtractUrlsService _extractUrlsService;

        public ExtractUrlsController(IExtractUrlsService extractUrlsService)
        {
            _extractUrlsService = extractUrlsService;
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
            var result = await _extractUrlsService.ExtractUrlsBySearch(search);

            if (result == null)
            {
                return NotFound("No data was found with the current keyword!");
            }

            return Ok(result);
        }
    }
}