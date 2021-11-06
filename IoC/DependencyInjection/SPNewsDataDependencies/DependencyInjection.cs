using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SPNewsData.Application.Entities.Mappings;
using SPNewsData.Data.Context;
using SPNewsData.Data.Repositories;
using SPNewsData.Domain.Interfaces;

namespace IoC.DependencyInjection.SPNewsDataDependencies
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddSPNewsData(this IServiceCollection services, IConfiguration configuration)
        {
            //AutoMapper
            services.AddAutoMapper(typeof(DomainMappingProfile));

            //Database
            string mySqlConnectionStr = configuration.GetConnectionString("DefaultConnection");
            services.AddDbContext<SPNewsDataContext>(options =>
            {
                options.UseMySql(mySqlConnectionStr, ServerVersion.AutoDetect(mySqlConnectionStr),
                                 mySqlOptionsAction: x => x.MigrationsAssembly($"{nameof(SPNewsData)}.Data"));
            }, ServiceLifetime.Transient);

            //Repositories
            services.AddScoped<IUrlExtractedRepository, UrlExtractedRepository>();

            //Health Check
            services.AddHealthChecks()
                .AddCheck<UrlExtractedRepository>($"{nameof(SPGovernmentData)}{nameof(UrlExtractedRepository)}");

            return services;
        }
    }
}