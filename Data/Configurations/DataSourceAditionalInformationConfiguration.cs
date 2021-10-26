using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Data.Configurations
{
    public class DataSourceAditionalInformationConfiguration : IEntityTypeConfiguration<DataSourceAditionalInformation>
    {
        public void Configure(EntityTypeBuilder<DataSourceAditionalInformation> builder)
        {
            builder.HasKey(x => x.Id);
        }
    }
}