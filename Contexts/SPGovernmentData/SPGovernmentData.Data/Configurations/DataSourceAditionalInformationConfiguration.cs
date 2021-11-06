using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SPGovernmentData.Domain.Entities;

namespace SPGovernmentData.Data.Configurations
{
    public class DataSourceAditionalInformationConfiguration : IEntityTypeConfiguration<DataSourceAditionalInformation>
    {
        public void Configure(EntityTypeBuilder<DataSourceAditionalInformation> builder)
        {
            builder.HasKey(x => x.Id);
        }
    }
}