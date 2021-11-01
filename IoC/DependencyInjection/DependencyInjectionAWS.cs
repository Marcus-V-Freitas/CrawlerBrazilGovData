using Amazon.S3;
using Amazon.SQS;
using AWSHelpers.S3.Implementation;
using AWSHelpers.S3.Interfaces;
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
            services.AddScoped<IS3Helper, S3Helper>();

            //AWS Options
            services.AddDefaultAWSOptions(configuration.GetAWSOptions("AWS"));
            services.AddAWSService<IAmazonSQS>();
            services.AddAWSService<IAmazonS3>();
            return services;
        }
    }
}