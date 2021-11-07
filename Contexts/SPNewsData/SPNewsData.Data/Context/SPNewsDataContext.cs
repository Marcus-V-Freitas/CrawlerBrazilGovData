using Microsoft.EntityFrameworkCore;
using SPNewsData.Domain.Entities;
using System.Reflection;

namespace SPNewsData.Data.Context
{
    public class SPNewsDataContext : DbContext
    {
        public SPNewsDataContext(DbContextOptions<SPNewsDataContext> options) : base(options)
        {
        }

        public DbSet<UrlExtracted> UrlExtracteds { get; set; }
        public DbSet<GovNews> GovNews { get; set; }
        public DbSet<Subject> Subjects { get; set; }
        public DbSet<Evidence> Evidences { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        }
    }
}