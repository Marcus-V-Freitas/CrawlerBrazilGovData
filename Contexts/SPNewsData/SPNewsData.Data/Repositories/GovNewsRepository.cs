using Microsoft.Extensions.Diagnostics.HealthChecks;
using MySqlConnector;
using SPNewsData.Data.Context;
using SPNewsData.Domain.Entities;
using SPNewsData.Domain.Interfaces;
using System.Threading;
using System.Threading.Tasks;

namespace SPNewsData.Data.Repositories
{
    public class GovNewsRepository : Repository<GovNews>, IGovNewsRepository, IHealthCheck
    {
        public GovNewsRepository(SPNewsDataContext context) : base(context)
        {
        }

        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
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