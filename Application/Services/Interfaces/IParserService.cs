using Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.Services.Interfaces
{
    public interface IParserService
    {
        Task<List<Dataset>> ParserUrlToDataset(List<UrlExtracted> urls);
    }
}