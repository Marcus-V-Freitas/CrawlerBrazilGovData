using Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.Services.Interfaces
{
    public interface IExtractUrlsService
    {
        Task<List<UrlExtracted>> ExtractUrlsBySearch(string search);

        Task<List<UrlExtracted>> GetUrlsBySearch(string search);
    }
}