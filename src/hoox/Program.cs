using System;
using System.Net;
using Google.Apis.Logging;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.Options;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace hoox
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureLogging(config => config
                    .AddConsole()
                    .AddDebug())
                .ConfigureAppConfiguration((host, config) =>
                {
                    config
                        .AddJsonFile("appSettings.json", optional: true)
                        .AddJsonFile($"appSettings.{host.HostingEnvironment.EnvironmentName}.json", optional: true)
                        .AddJsonFile($"appSettings.local.json", optional: true)
                        .AddEnvironmentVariables(prefix: "HOOX_")
                        .AddCommandLine(args);
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
