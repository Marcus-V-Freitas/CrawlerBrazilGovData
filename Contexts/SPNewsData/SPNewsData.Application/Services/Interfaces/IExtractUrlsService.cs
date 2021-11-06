using SPNewsData.Application.Entities.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SPNewsData.Application.Services.Interfaces
{
    public interface IExtractUrlsService
    {
        Task<List<UrlExtractedDTO>> ExtractUrlsBySearch(string search);

        Task<List<UrlExtractedDTO>> GetUrlsBySearch(string search);
    }
}