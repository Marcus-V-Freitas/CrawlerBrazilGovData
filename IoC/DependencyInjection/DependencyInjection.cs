using Core.Cache.Implementation;
using Core.Cache.Interfaces;
using Core.Configuration;
using IoC.DependencyInjection.SPGovernmentDataDependencies;
using IoC.Middlewares;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using SPGovernmentData.Application.Entities.Mappings;
using System.Net.Http;

namespace IoC.DependencyInjection
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            //AutoMapper
            services.AddAutoMapper(typeof(DomainMappingProfile));

            //Exception
            services.AddTransient<GlobalExceptionHandlerMiddleware>();

            //General
            services.AddScoped<HttpClient>();
            services.AddControllers();

            //SPGovernmentData
            services.AddSPGovernmentData(configuration);

            //JSON settings
            JsonConvert.DefaultSettings = () => new JsonSerializerSettings
            {
                Formatting = Formatting.None,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                TypeNameHandling = TypeNameHandling.Objects,
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };

            //Memory Cache
            services.AddMemoryCache();
            services.AddSingleton<ICacheProvider, CacheProvider>();

            //Options
            services.Configure<Configs>(configuration.GetSection("Configs"));
            services.AddOptions();

            return services;
        }
    }
}