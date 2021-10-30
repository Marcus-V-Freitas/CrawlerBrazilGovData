using Amazon.SQS;
using AWSHelpers.SQS.Implementation;
using AWSHelpers.SQS.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace IoC.DependencyInjection
{
    public static class DependencyInjectionAWS
    {
        public static IServiceCollection AddAWS(this IServiceCollection services, IConfiguration configuration)
        {
            //AWS Dependencies
            services.AddScoped<ISQSHelper, SQSHelper>();

            //AWS Options
            services.AddDefaultAWSOptions(configuration.GetAWSOptions("SQS"));
            services.AddAWSService<IAmazonSQS>();
            return services;
        }
    }
}