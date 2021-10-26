using Application.Services.Implementations;
using Application.Services.Interfaces;
using Data.Context;
using Data.Repositories;
using Domain.Interfaces;
using IoC.Middlewares;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http;

namespace IoC.DependencyInjection
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            //Database
            string mySqlConnectionStr = configuration.GetConnectionString("DefaultConnection");
            services.AddDbContextPool<CrawlerBrazilGovDataContext>(options =>
            {
                options.UseMySql(mySqlConnectionStr, ServerVersion.AutoDetect(mySqlConnectionStr),
                                 mySqlOptionsAction: x => x.MigrationsAssembly(nameof(Data)));
            });

            //Exceptions
            services.AddScoped<HttpClient>();
            services.AddTransient<GlobalExceptionHandlerMiddleware>();

            //Repositories
            services.AddScoped<ITagRepository, TagRepository>();
            services.AddScoped<IDatasetRepository, DatasetRepository>();
            services.AddScoped<IDataSourceRepository, DataSourceRepository>();
            services.AddScoped<IUrlExtractedRepository, UrlExtractedRepository>();
            services.AddScoped<IDataSourceAditionalInformationRepository, DataSourceAditionalInformationRepository>();
            services.AddScoped<IDatasetAditionalInformationRepository, DatasetAditionalInformationRepository>();

            //Services
            services.AddScoped<IExtractUrlsService, ExtractUrlsService>();
            services.AddScoped<IParserService, ParserService>();

            //Health Check
            services.AddHealthChecks()
                .AddCheck<TagRepository>(nameof(TagRepository))
                .AddCheck<DatasetRepository>(nameof(DatasetRepository))
                .AddCheck<DataSourceRepository>(nameof(DataSourceRepository))
                .AddCheck<DataSourceAditionalInformationRepository>(nameof(DataSourceAditionalInformationRepository))
                .AddCheck<DatasetAditionalInformationRepository>(nameof(DatasetAditionalInformationRepository));

            return services;
        }
    }
}