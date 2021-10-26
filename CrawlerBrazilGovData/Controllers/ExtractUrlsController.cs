using Application.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace CrawlerBrazilGovData.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ExtractUrlsController : Controller
    {
        private readonly IExtractUrlsService _extractUrlsService;

        public ExtractUrlsController(IExtractUrlsService extractUrlsService)
        {
            _extractUrlsService = extractUrlsService;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="search"></param>
        /// <returns></returns>
        [HttpGet("", Name = nameof(ExtractUrlsBySearch))]
        public async Task<IActionResult> ExtractUrlsBySearch([FromQuery] string search)
        {
            var result = await _extractUrlsService.ExtractUrlsBySearch(search);
            return Ok(result);
        }
    }
}