using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace Data.Context
{
    public class CrawlerBrazilGovDataContext : DbContext
    {
        public CrawlerBrazilGovDataContext(DbContextOptions<CrawlerBrazilGovDataContext> options) : base(options)
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