using Application.Entities.Configuration;
using Application.Entities.Mappings;
using Application.Services.Implementations;
using Application.Services.Interfaces;
using Core.Cache.Implementation;
using Core.Cache.Interfaces;
using Data.Context;
using Data.Repositories;
using Domain.Interfaces;
using IoC.Middlewares;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Net.Http;

namespace IoC.DependencyInjection
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            //Database
            string mySqlConnectionStr = configuration.GetConnectionString("DefaultConnection");
            services.AddDbContext<CrawlerBrazilGovDataContext>(options =>
            {
                options.UseMySql(mySqlConnectionStr, ServerVersion.AutoDetect(mySqlConnectionStr),
                                 mySqlOptionsAction: x => x.MigrationsAssembly(nameof(Data)));
            }, ServiceLifetime.Transient);

            //AutoMapper
            services.AddAutoMapper(typeof(DomainMappingProfile));

            //Exception
            services.AddTransient<GlobalExceptionHandlerMiddleware>();

            //General
            services.AddScoped<HttpClient>();
            services.AddControllers();

            //JSON settings
            JsonConvert.DefaultSettings = () => new JsonSerializerSettings
            {
                Formatting = Formatting.None,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                TypeNameHandling = TypeNameHandling.Objects,
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };

            //Repositories
            services.AddScoped<ITagRepository, TagRepository>();
            services.AddScoped<IDatasetRepository, DatasetRepository>();
            services.AddScoped<IDataSourceRepository, DataSourceRepository>();
            services.AddScoped<IUrlExtractedRepository, UrlExtractedRepository>();
            services.AddScoped<IDataSourceAditionalInformationRepository, DataSourceAditionalInformationRepository>();
            services.AddScoped<IDatasetAditionalInformationRepository, DatasetAditionalInformationRepository>();

            //Memory Cache
            services.AddMemoryCache();
            services.AddSingleton<ICacheProvider, CacheProvider>();

            //Services
            services.AddScoped<IExtractUrlsService, ExtractUrlsService>();
            services.AddScoped<IParserService, ParserService>();

            //Options
            services.Configure<Configs>(configuration.GetSection("Configs"));
            services.AddOptions();

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