using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SPGovernmentData.Application.Entities.Mappings;
using SPGovernmentData.Application.Services.Implementations;
using SPGovernmentData.Application.Services.Interfaces;
using SPGovernmentData.Data.Context;
using SPGovernmentData.Data.Repositories;
using SPGovernmentData.Domain.Interfaces;

namespace IoC.DependencyInjection.SPGovernmentDataDependencies
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddSPGovernmentData(this IServiceCollection services, IConfiguration configuration)
        {
            //AutoMapper
            services.AddAutoMapper(typeof(DomainMappingProfile));

            //Database
            string mySqlConnectionStr = configuration.GetConnectionString("DefaultConnection");
            services.AddDbContext<SPGovernmentDataContext>(options =>
            {
                options.UseMySql(mySqlConnectionStr, ServerVersion.AutoDetect(mySqlConnectionStr),
                                 mySqlOptionsAction: x => x.MigrationsAssembly($"{nameof(SPGovernmentData)}.Data"));
            }, ServiceLifetime.Transient);

            //Repositories
            services.AddScoped<ITagRepository, TagRepository>();
            services.AddScoped<IDatasetRepository, DatasetRepository>();
            services.AddScoped<IDataSourceRepository, DataSourceRepository>();
            services.AddScoped<IUrlExtractedRepository, UrlExtractedRepository>();
            services.AddScoped<IDataSourceAditionalInformationRepository, DataSourceAditionalInformationRepository>();
            services.AddScoped<IDatasetAditionalInformationRepository, DatasetAditionalInformationRepository>();

            //Health Check
            services.AddHealthChecks()
                .AddCheck<UrlExtractedRepository>($"{nameof(SPGovernmentData)}{nameof(UrlExtractedRepository)}")
                .AddCheck<TagRepository>($"{nameof(SPGovernmentData)}{ nameof(TagRepository)}")
                .AddCheck<DatasetRepository>($"{nameof(SPGovernmentData)}{nameof(DatasetRepository)}")
                .AddCheck<DataSourceRepository>($"{nameof(SPGovernmentData)}{nameof(DataSourceRepository)}")
                .AddCheck<DataSourceAditionalInformationRepository>($"{nameof(SPGovernmentData)}{nameof(DataSourceAditionalInformationRepository)}")
                .AddCheck<DatasetAditionalInformationRepository>($"{nameof(SPGovernmentData)}{nameof(DatasetAditionalInformationRepository)}");

            //Services
            services.AddScoped<IExtractUrlsService, ExtractUrlsService>();
            services.AddScoped<IParserService, ParserService>();

            return services;
        }
    }
}