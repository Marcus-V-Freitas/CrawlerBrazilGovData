using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Data.Configurations
{
    public class UrlExtractedConfiguration : IEntityTypeConfiguration<UrlExtracted>
    {
        public void Configure(EntityTypeBuilder<UrlExtracted> builder)
        {
            builder.HasKey(x => x.Id);
        }
    }
}