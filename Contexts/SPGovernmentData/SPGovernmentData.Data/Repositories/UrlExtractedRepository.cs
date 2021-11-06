﻿using Microsoft.Extensions.Diagnostics.HealthChecks;
using MySql.Data.MySqlClient;
using SPGovernmentData.Data.Context;
using SPGovernmentData.Domain.Entities;
using SPGovernmentData.Domain.Interfaces;
using System.Threading;
using System.Threading.Tasks;

namespace SPGovernmentData.Data.Repositories
{
    public class UrlExtractedRepository : Repository<UrlExtracted>, IUrlExtractedRepository, IHealthCheck
    {
        public UrlExtractedRepository(SPGovernmentDataContext context) : base(context)
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