using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SPNewsData.Domain.Entities;

namespace SPNewsData.Data.Configurations
{
    public class SubjectConfiguration : IEntityTypeConfiguration<Subject>
    {
        public void Configure(EntityTypeBuilder<Subject> builder)
        {
            builder.HasKey(x => x.Id);

            builder.HasOne(x => x.GovNews)
                .WithMany(x => x.Subjects)
                .HasForeignKey(x => x.GovNewsId);
        }
    }
}