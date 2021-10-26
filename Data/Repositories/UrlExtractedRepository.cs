using Data.Context;
using Domain.Entities;
using Domain.Interfaces;

namespace Data.Repositories
{
    public class UrlExtractedRepository : Repository<UrlExtracted>, IUrlExtractedRepository
    {
        public UrlExtractedRepository(CrawlerBrazilGovDataContext context) : base(context)
        {
        }
    }
}