using Microsoft.EntityFrameworkCore;
using SPGovernmentData.Domain.Entities;
using System.Reflection;

namespace SPGovernmentData.Data.Context
{
    public class SPGovernmentDataContext : DbContext
    {
        public SPGovernmentDataContext(DbContextOptions<SPGovernmentDataContext> options) : base(options)
        {
        }

        public DbSet<Tag> Tags { get; set; }
        public DbSet<Dataset> Datasets { get; set; }
        public DbSet<DataSource> DataSources { get; set; }
        public DbSet<UrlExtracted> UrlExtracteds { get; set; }
        public DbSet<DatasetAditionalInformation> DatasetAditionalInformations { get; set; }
        public DbSet<DataSourceAditionalInformation> DataSourceAditionalInformations { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        }
    }
}