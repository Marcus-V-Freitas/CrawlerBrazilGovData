using SPGovernmentData.Application.Entities.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SPGovernmentData.Application.Services.Interfaces
{
    public interface IParserService
    {
        Task<List<DatasetDTO>> ParserUrlToDataset(string search);
    }
}