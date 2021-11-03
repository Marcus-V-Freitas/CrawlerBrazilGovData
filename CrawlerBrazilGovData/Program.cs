using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using System;
using System.IO;
using System.Reflection;

namespace CrawlerBrazilGovData
{
    public class Program
    {
        public static void Main(string[] args)
        {
            string log = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "Logs.txt");

            Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Error)
            .Enrich.FromLogContext()
            .WriteTo.File(log)
            .CreateLogger();

            try
            {
                Log.Information("--- SERVER STARTING ---");
                CreateHostBuilder(args).Build().Run();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "--- SERVER ENDED UNEXPECTEDLY ---");
            }
            finally
            {
                Log.Information("--- SERVER ENDING ---");
                Log.CloseAndFlush();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                }).UseSerilog();
    }
}