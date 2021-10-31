using Application.Entities.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.Services.Interfaces
{
    public interface IParserService
    {
        Task<List<DatasetDTO>> ParserUrlToDataset(string search);
    }
}