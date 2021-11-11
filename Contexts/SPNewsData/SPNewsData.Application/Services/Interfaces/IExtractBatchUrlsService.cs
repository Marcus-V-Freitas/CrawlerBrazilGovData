using SPNewsData.Application.Entities.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SPNewsData.Application.Services.Interfaces
{
    public interface IExtractBatchUrlsService
    {
        Task<List<UrlExtractedDTO>> ExtractUrlsBySearch();

        Task<List<UrlExtractedDTO>> GetUrlsBySearch();
    }
}