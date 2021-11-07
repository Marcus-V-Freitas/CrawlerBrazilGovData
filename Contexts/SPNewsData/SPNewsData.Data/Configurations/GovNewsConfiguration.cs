using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SPNewsData.Domain.Entities;

namespace SPNewsData.Data.Configurations
{
    public class GovNewsConfiguration : IEntityTypeConfiguration<GovNews>
    {
        public void Configure(EntityTypeBuilder<GovNews> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(p => p.Content)
                    .HasColumnType("text");

            builder.Property(x => x.CaptureDate)
                    .HasDefaultValueSql("(CURRENT_TIMESTAMP)");
        }
    }
}