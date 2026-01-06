using Dilcore.Telemetry.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Logs;

namespace Dilcore.Telemetry.Extensions.OpenTelemetry;

/// <summary>
/// Unified processor that enriches OpenTelemetry logs with attributes from all registered providers.
/// </summary>
public class UnifiedLogRecordProcessor : BaseProcessor<LogRecord>
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<UnifiedLogRecordProcessor> _logger;

    public UnifiedLogRecordProcessor(
        IServiceProvider serviceProvider,
        ILogger<UnifiedLogRecordProcessor> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public override void OnEnd(LogRecord data)
    {
        var attributes = data.Attributes?.ToList() ?? [];

        try
        {
            // Create a scope to resolve scoped services (like UserAttributeProvider)
            using var scope = _serviceProvider.CreateScope();
            var attributeProviders = scope.ServiceProvider.GetServices<ITelemetryAttributeProvider>();

            // Collect attributes from all providers
            foreach (var provider in attributeProviders)
            {
                try
                {
                    var providerAttributes = provider.GetAttributes();
                    foreach (var attribute in providerAttributes)
                    {
                        // Remove existing attribute with same key to avoid duplicates
                        attributes.RemoveAll(kv => kv.Key == attribute.Key);
                        attributes.Add(attribute);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to get attributes from provider {ProviderName}", provider.GetType().Name);
                }
            }

            data.Attributes = attributes;
        }
        finally
        {
            // Always call base.OnEnd even if enrichment fails
            base.OnEnd(data);
        }
    }
}