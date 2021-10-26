using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Data.Configurations
{
    public class DatasetConfiguration : IEntityTypeConfiguration<Dataset>
    {
        public void Configure(EntityTypeBuilder<Dataset> builder)
        {
            builder.HasKey(x => x.Id);

            builder.HasOne(x => x.AditionalInformation)
                .WithMany()
                .HasForeignKey(x => x.AditionalInformationId);
        }
    }
}