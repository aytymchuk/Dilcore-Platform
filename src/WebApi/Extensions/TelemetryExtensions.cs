using Azure.Monitor.OpenTelemetry.AspNetCore;
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
        services.AddSingleton<TenantAndUserContextProcessor>();
        services.AddSingleton<TenantAndUserActivityProcessor>();

        services.ConfigureOpenTelemetryTracerProvider((sp, tpBuilder) =>
        {
            tpBuilder.AddProcessor(sp.GetRequiredService<TenantAndUserActivityProcessor>());
        });

        services.ConfigureOpenTelemetryLoggerProvider((sp, lpBuilder) =>
        {
            lpBuilder.AddProcessor(sp.GetRequiredService<TenantAndUserContextProcessor>());
        });

        var otel = services.AddOpenTelemetry()
            .ConfigureResource(resource => resource.AddService(env.ApplicationName, serviceVersion: serviceVersion));

        // Unify instrumentation and logging configuration
        otel.WithTracing(tracing =>
        {
            tracing.AddAspNetCoreInstrumentation();
            tracing.AddHttpClientInstrumentation();
        })
        .WithMetrics(metrics =>
        {
            metrics.AddAspNetCoreInstrumentation();
            metrics.AddHttpClientInstrumentation();
        })
        .WithLogging();

        if (!string.IsNullOrEmpty(settings.ApplicationInsightsConnectionString))
        {
            // UseAzureMonitor handles its own instrumentation but respects the base configuration above.
            otel.UseAzureMonitor(options => options.ConnectionString = settings.ApplicationInsightsConnectionString);
        }
        else
        {
            // Local development exporters
            otel.WithTracing(tracing => tracing.AddConsoleExporter())
                .WithMetrics(metrics => metrics.AddConsoleExporter())
                .WithLogging(logging => logging.AddConsoleExporter());
        }

        return services;
    }
}