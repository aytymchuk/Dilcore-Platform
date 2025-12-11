using Azure.Monitor.OpenTelemetry.AspNetCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Logs;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OpenTelemetry.Metrics;

using Microsoft.AspNetCore.Http;

namespace WebApi.Extensions;

public static class TelemetryExtensions
{
    public static IServiceCollection AddTelemetry(this IServiceCollection services, IConfiguration configuration)
    {
        var settings = configuration.GetSection("Telemetry").Get<TelemetrySettings>() ?? new TelemetrySettings();
        var serviceVersion = Environment.GetEnvironmentVariable("BUILD_VERSION") ?? "local_development";

        services.AddHttpContextAccessor();
        services.AddSingleton<TenantAndUserContextProcessor>();
        services.AddSingleton<TenantAndUserActivityProcessor>();

        var otel = services.AddOpenTelemetry()
            .ConfigureResource(resource => resource.AddService(settings.ServiceName, serviceVersion: serviceVersion))
            .WithLogging(logging => 
            {
                logging.AddConsoleExporter();
                logging.AddProcessor(sp => sp.GetRequiredService<TenantAndUserContextProcessor>());
            });

        if (!string.IsNullOrEmpty(settings.ConnectionString))
        {
            otel.UseAzureMonitor(options => options.ConnectionString = settings.ConnectionString);
        }
        else
        {
            otel.WithTracing(tracing =>
            {
                tracing.AddAspNetCoreInstrumentation();
                tracing.AddHttpClientInstrumentation();
                tracing.AddProcessor(sp => sp.GetRequiredService<TenantAndUserActivityProcessor>());
            })
            .WithMetrics(metrics =>
            {
                metrics.AddAspNetCoreInstrumentation();
                metrics.AddHttpClientInstrumentation();
            });
        }

        return services;
    }
}