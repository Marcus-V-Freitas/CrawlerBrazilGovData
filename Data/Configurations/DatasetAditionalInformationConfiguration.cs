using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Data.Configurations
{
    public class DatasetAditionalInformationConfiguration : IEntityTypeConfiguration<DatasetAditionalInformation>
    {
        public void Configure(EntityTypeBuilder<DatasetAditionalInformation> builder)
        {
            builder.HasKey(x => x.Id);
        }
    }
}