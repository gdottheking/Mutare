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

namespace Sharara.Services.Kumusha
{

    class Program
    {
        public static async Task DatabaseTest()
        {
            var db = new DbCtx();
            db.Database.EnsureCreated();

            Console.WriteLine("Inserting address");
            var repo = new Generated.Repository(db);
            var ent = new Generated.AddressEntity
            {
                Line1 = new Random(DateTime.Now.Second).Next(100) + " Fairview Road",
                PostalCode = "GU12 6AS"
            };
            await repo.PutAddressAsync(ent);
            Console.WriteLine($"Address added with id = {ent.Id}");

            var numAddr = await repo.GetAddressCountAsync();
            Console.WriteLine($"There are {numAddr} addresses in the database");
            var addresses = await repo.GetAllAddressesAsync(0, 1000);
            foreach (var addr in addresses)
            {
                Console.WriteLine(addr.Line1 + " " + addr.PostalCode);
            }
        }

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
            // string cfgPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "appsettings.common.json");
            // builder.AddJsonFile(cfgPath, optional: false);
        }

    }
}