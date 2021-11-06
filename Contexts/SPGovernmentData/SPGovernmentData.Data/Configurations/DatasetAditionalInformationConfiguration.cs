using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SPGovernmentData.Domain.Entities;

namespace SPGovernmentData.Data.Configurations
{
    public class DatasetAditionalInformationConfiguration : IEntityTypeConfiguration<DatasetAditionalInformation>
    {
        public void Configure(EntityTypeBuilder<DatasetAditionalInformation> builder)
        {
            builder.HasKey(x => x.Id);
        }
    }
}