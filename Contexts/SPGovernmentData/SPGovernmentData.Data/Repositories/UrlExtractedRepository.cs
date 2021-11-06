using SPGovernmentData.Data.Context;
using SPGovernmentData.Domain.Entities;
using SPGovernmentData.Domain.Interfaces;

namespace SPGovernmentData.Data.Repositories
{
    public class UrlExtractedRepository : Repository<UrlExtracted>, IUrlExtractedRepository
    {
        public UrlExtractedRepository(SPGovernmentDataContext context) : base(context)
        {
        }
    }
}