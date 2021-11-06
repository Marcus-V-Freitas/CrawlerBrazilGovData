using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SPGovernmentData.Domain.Entities;

namespace SPGovernmentData.Data.Configurations
{
    public class DataSourceConfiguration : IEntityTypeConfiguration<DataSource>
    {
        public void Configure(EntityTypeBuilder<DataSource> builder)
        {
            builder.HasKey(x => x.Id);

            builder.HasOne(x => x.AditionalInformation)
                .WithMany()
                .HasForeignKey(x => x.AditionalInformationId);

            builder.HasOne(x => x.Dataset)
                .WithMany(y => y.DataSources)
                .HasForeignKey(x => x.DatasetId);
        }
    }
}