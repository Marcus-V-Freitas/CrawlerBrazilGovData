using Application.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace CrawlerBrazilGovData.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ParserController : ControllerBase
    {
        private readonly IExtractUrlsService _extractUrlsService;
        private readonly IParserService _parserService;

        public ParserController(IExtractUrlsService extractUrlsService, IParserService parserService)
        {
            _extractUrlsService = extractUrlsService;
            _parserService = parserService;
        }

        [HttpGet("")]
        public async Task<IActionResult> ParserBySearch([FromQuery] string search)
        {
            var urls = await _extractUrlsService.GetUrlsBySearch(search);

            if (urls != null)
            {
                return Ok(await _parserService.ParserUrlToDataset(urls));
            }

            return NotFound();
        }
    }
}