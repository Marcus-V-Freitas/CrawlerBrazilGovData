using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SPNewsData.Application.Entities.Mappings;
using SPNewsData.Application.Services.Implementations;
using SPNewsData.Application.Services.Interfaces;
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
            services.AddScoped<IGovNewsRepository, GovNewsRepository>();
            services.AddScoped<ISubjectRepository, SubjectRepository>();
            services.AddScoped<IEvidenceRepository, EvidenceRepository>();

            //Health Check
            services.AddHealthChecks()
                .AddCheck<UrlExtractedRepository>($"{nameof(SPNewsData)}{nameof(UrlExtractedRepository)}")
                .AddCheck<GovNewsRepository>($"{nameof(SPNewsData)}{nameof(GovNewsRepository)}")
                .AddCheck<SubjectRepository>($"{nameof(SPNewsData)}{nameof(SubjectRepository)}")
                .AddCheck<EvidenceRepository>($"{nameof(SPNewsData)}{nameof(EvidenceRepository)}");

            //Services
            services.AddScoped<IExtractUrlsService, ExtractUrlsService>();
            services.AddScoped<IParserService, ParseService>();

            return services;
        }
    }
}