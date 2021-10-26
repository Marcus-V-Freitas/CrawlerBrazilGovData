using Data.Context;
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using MySqlConnector;
using System.Threading;
using System.Threading.Tasks;

namespace Data.Repositories
{
    public class TagRepository : Repository<Tag>, ITagRepository, IHealthCheck
    {
        public TagRepository(CrawlerBrazilGovDataContext context) : base(context)
        {
        }

        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext healthContext, CancellationToken cancellationToken = default)
        {
            using (var db = new MySqlConnection(ConnectionString))
            {
                try
                {
                    db.Open();
                    db.Close();
                    return Task.FromResult(HealthCheckResult.Healthy());
                }
                catch
                {
                    return Task.FromResult(HealthCheckResult.Unhealthy());
                }
            }
        }
    }
}