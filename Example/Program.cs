using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using App.Metrics;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using System.Reflection;
using System.IO;
using App.Metrics.AspNetCore;
using App.Metrics.Formatters.Prometheus;
using Microsoft.AspNetCore.Mvc;

[assembly: ApiController]

namespace Sharara.Services.Kumusha
{

    class Program
    {
        public static IMetricsRoot Metrics { get; private set; }

        public static void Main(string[] args)
        {
            Program.CreateHostBuilder<Startup>(args).Build().Run();
        }

        // Additional configuration is required to successfully run gRPC on macOS.
        // For instructions on how to configure Kestrel and gRPC clients on macOS, visit https://go.microsoft.com/fwlink/?linkid=2099682
        public static IHostBuilder CreateHostBuilder<TStartup>(string[] args)
            where TStartup : class
        {
            Metrics = AppMetrics.CreateDefaultBuilder()
                .OutputMetrics.AsPrometheusPlainText()
                .OutputMetrics.AsPrometheusProtobuf()
                .Build();

            return Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((ctx, builder) => Configure(builder))
                .UseMetrics(options =>
                {
                    options.EndpointOptions = endpointsOptions =>
                    {
                        endpointsOptions.MetricsTextEndpointOutputFormatter = Metrics.OutputMetricsFormatters.OfType<MetricsPrometheusTextOutputFormatter>().First();
                        endpointsOptions.MetricsEndpointOutputFormatter = Metrics.OutputMetricsFormatters.OfType<MetricsPrometheusProtobufOutputFormatter>().First();
                    };
                })
                .UseMetricsWebTracking()
                .UseMetricsEndpoints()
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<TStartup>();
                });
        }

        public static void Configure(IConfigurationBuilder builder)
        {
            // string appSettingsPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "appsettings.json");
            // builder.AddJsonFile(appSettingsPath, optional: false);

            // string devAppSettingsPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "appsettings.Development.json");
            // builder.AddJsonFile(appSettingsPath, optional: false);
        }

    }
}