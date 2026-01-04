using Azure.Monitor.OpenTelemetry.AspNetCore;
using Dilcore.WebApi.Infrastructure.MultiTenant;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Dilcore.WebApi.Settings;

namespace Dilcore.WebApi.Extensions;

public static class TelemetryExtensions
{
    public static IServiceCollection AddTelemetry(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment env)
    {
        var settings = configuration.GetSettings<TelemetrySettings>();
        var serviceVersion = configuration.GetValueOrDefault(Constants.Configuration.BuildVersionKey, Constants.Configuration.DefaultBuildVersion);

        services.AddHttpContextAccessor();
        services.AddSingleton<TenantContextProcessor>();
        services.AddSingleton<UserContextProcessor>();
        services.AddSingleton<TenantActivityProcessor>();
        services.AddSingleton<UserActivityProcessor>();

        services.ConfigureOpenTelemetryTracerProvider((sp, tpBuilder) =>
        {
            tpBuilder.AddProcessor(sp.GetRequiredService<TenantActivityProcessor>());
            tpBuilder.AddProcessor(sp.GetRequiredService<UserActivityProcessor>());
        });

        services.ConfigureOpenTelemetryLoggerProvider((sp, lpBuilder) =>
        {
            lpBuilder.AddProcessor(sp.GetRequiredService<TenantContextProcessor>());
            lpBuilder.AddProcessor(sp.GetRequiredService<UserContextProcessor>());
        });

        var otel = services.AddOpenTelemetry()
            .ConfigureResource(resource => resource.AddService(env.ApplicationName, serviceVersion: serviceVersion));

        if (!string.IsNullOrEmpty(settings.ApplicationInsightsConnectionString))
        {
            // UseAzureMonitor handles its own instrumentation (AspNetCore, HttpClient, etc.)
            otel.UseAzureMonitor(options => options.ConnectionString = settings.ApplicationInsightsConnectionString);
        }
        else
        {
            // Local development: add instrumentation + console exporters
            otel.WithTracing(tracing =>
                {
                    tracing.AddAspNetCoreInstrumentation();
                    tracing.AddHttpClientInstrumentation();
                    tracing.AddConsoleExporter();
                })
                .WithMetrics(metrics =>
                {
                    metrics.AddAspNetCoreInstrumentation();
                    metrics.AddHttpClientInstrumentation();
                    metrics.AddConsoleExporter();
                })
                .WithLogging(logging => logging.AddConsoleExporter());
        }

        return services;
    }
}