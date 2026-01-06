using Azure.Monitor.OpenTelemetry.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Dilcore.OpenTelemetry.Extensions;

public static class TelemetryExtensions
{
    public static IServiceCollection AddTelemetry(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment env)
    {
        var settings = configuration.GetSection(nameof(TelemetrySettings)).Get<TelemetrySettings>() ?? new TelemetrySettings();
        services.AddSingleton(settings);
        var serviceVersion = configuration[Constants.BuildVersionKey] ?? Constants.DefaultBuildVersion;

        services.AddHttpContextAccessor();

        services.AddHttpContextAccessor();

        // Register unified processors
        services.AddSingleton<UnifiedLogRecordProcessor>();
        services.AddSingleton<UnifiedActivityProcessor>();

        services.ConfigureOpenTelemetryTracerProvider((sp, tpBuilder) =>
        {
            tpBuilder.AddProcessor(sp.GetRequiredService<UnifiedActivityProcessor>());
        });

        services.ConfigureOpenTelemetryLoggerProvider((sp, lpBuilder) =>
        {
            lpBuilder.AddProcessor(sp.GetRequiredService<UnifiedLogRecordProcessor>());
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