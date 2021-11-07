using SPNewsData.Application.Entities.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SPNewsData.Application.Services.Interfaces
{
    public interface IParserService
    {
        Task<List<GovNewsDTO>> ParserUrlToGovNews(string search);
    }
}