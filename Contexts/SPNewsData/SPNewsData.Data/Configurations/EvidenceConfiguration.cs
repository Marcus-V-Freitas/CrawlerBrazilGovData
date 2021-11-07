using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SPNewsData.Domain.Entities;

namespace SPNewsData.Data.Configurations
{
    public class EvidenceConfiguration : IEntityTypeConfiguration<Evidence>
    {
        public void Configure(EntityTypeBuilder<Evidence> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.RawContent)
                 .HasColumnType("text");

            builder.Property(m => m.EvidenceType)
                 .HasConversion<int>();

            builder.HasOne(x => x.GovNews)
                .WithMany(x => x.Evidences)
                .HasForeignKey(x => x.GovNewsId);
        }
    }
}