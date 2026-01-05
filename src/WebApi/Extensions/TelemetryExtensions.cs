using Azure.Monitor.OpenTelemetry.AspNetCore;
using Dilcore.MultiTenant.Http.Extensions;
using Dilcore.MultiTenant.Http.Extensions.Telemetry;
using Dilcore.Telemetry.Abstractions;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Dilcore.WebApi.Settings;
using OpenTelemetry.Instrumentation.AspNetCore;

namespace Dilcore.WebApi.Extensions;

public static class TelemetryExtensions
{
    public static IServiceCollection AddTelemetry(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment env)
    {
        var settings = configuration.GetSettings<TelemetrySettings>();
        var serviceVersion = configuration.GetValueOrDefault(Constants.Configuration.BuildVersionKey, Constants.Configuration.DefaultBuildVersion);

        services.AddHttpContextAccessor();

        // Register attribute providers
        services.AddSingleton<ITelemetryAttributeProvider, UserAttributeProvider>();
        services.AddSingleton<ITelemetryAttributeProvider, TenantAttributeProvider>();

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

        services.AddTelemetryEnricher<TenantTelemetryEnricher>();
        services.AddTelemetryEnricher<UserTelemetryEnricher>();

        services.Configure<AspNetCoreTraceInstrumentationOptions>(options =>
        {
            options.EnrichWithHttpRequest = (activity, request) =>
            {
                var enrichers = request.HttpContext.RequestServices.GetServices<ITelemetryEnricher>();
                foreach (var enricher in enrichers)
                {
                    enricher.Enrich(activity, request);
                }
            };
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