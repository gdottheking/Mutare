using App.Metrics;
using Grpc.AspNetCore.Server;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;

namespace Sharara.Services.Kumusha
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddOptions();
            services.AddGrpc(ConfigureGrpc);
            services.AddAuthorization();
            services.AddGrpcReflection();
            services.AddControllers();
            services.AddMetrics(ConfigureAppMetrics);
            services.AddMetricsEndpoints();
            services.AddMetricsTrackingMiddleware();
            services.AddMetricsReportingHostedService();
            services.AddSingleton<Generated.DatabaseContext, DatabaseContextOverride>();
            services.AddSingleton<Generated.IRepository, Generated.Repository>();
            services.AddHealthChecks();
            services.AddSingleton<ClientDriver>();
        }

        protected virtual void ConfigureAppMetrics(IMetricsBuilder builder)
        {
            builder.OutputMetrics.AsPrometheusPlainText();
        }

        protected virtual void ConfigureGrpc(GrpcServiceOptions options)
        {
            // options.ResponseCompressionAlgorithm = "gzip";
            // options.ResponseCompressionLevel = System.IO.Compression.CompressionLevel.Optimal;
            // options.EnableDetailedErrors = true;
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app)
        {
            app.UseRouting();
            app.UseAuthorization();
            app.UseMetricsAllMiddleware();
            app.UseEndpoints(builder => ConfigureEndpoints(builder, app.ApplicationServices));
        }

        protected virtual void ConfigureEndpoints(IEndpointRouteBuilder endpoints, IServiceProvider serviceProvider)
        {
            // if (serviceProvider.GetService<IWebHostEnvironment>().IsDevelopment())
            // {
            //     endpoints.MapGrpcReflectionService();
            // }

            //endpoints.MapGrpcService<Generated.Service>();
            endpoints.MapControllers();
            //endpoints.MapControllerRoute("default", "{controller=Address}/{action=Index}/{id?}");
            endpoints.MapHealthChecks("/health");
            endpoints.MapGet("/", async context =>
            {
                var msg = "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909";
                await context.Response.WriteAsync(msg);
            });

        }

    }

}
