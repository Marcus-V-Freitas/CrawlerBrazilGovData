using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace IoC.DependencyInjection
{
    public static class DependencyInjectionSwagger
    {
        public static IServiceCollection AddInfrastructureSwagger(this IServiceCollection services, string xmlPath)
        {
            services.AddSwaggerGen(opt =>
            {
                //Include XML Controller Documentation
                if (!string.IsNullOrEmpty(xmlPath))
                {
                    opt.IncludeXmlComments(xmlPath);
                }

                opt.SwaggerDoc("v1", new OpenApiInfo { Title = "ArquiteturaLimpaMVC.API", Version = "v1" });
                opt.AddSecurityDefinition("Bearer", new()
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "JWT Authorization header with bearer HTTP."
                });
                opt.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference()
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    new string[] { }
                }
                });
            });

            return services;
        }
    }
}