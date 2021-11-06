using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SPGovernmentData.Domain.Entities;

namespace SPGovernmentData.Data.Configurations
{
    public class UrlExtractedConfiguration : IEntityTypeConfiguration<UrlExtracted>
    {
        public void Configure(EntityTypeBuilder<UrlExtracted> builder)
        {
            builder.HasKey(x => x.Id);
        }
    }
}