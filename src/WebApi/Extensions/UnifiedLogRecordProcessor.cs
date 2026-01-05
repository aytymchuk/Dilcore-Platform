using Dilcore.Telemetry.Abstractions;
using OpenTelemetry;
using OpenTelemetry.Logs;

namespace Dilcore.WebApi.Extensions;

/// <summary>
/// Unified processor that enriches OpenTelemetry logs with attributes from all registered providers.
/// </summary>
public class UnifiedLogRecordProcessor : BaseProcessor<LogRecord>
{
    private readonly IEnumerable<ITelemetryAttributeProvider> _attributeProviders;
    private readonly ILogger<UnifiedLogRecordProcessor> _logger;

    public UnifiedLogRecordProcessor(
        IEnumerable<ITelemetryAttributeProvider> attributeProviders,
        ILogger<UnifiedLogRecordProcessor> logger)
    {
        _attributeProviders = attributeProviders;
        _logger = logger;
    }

    public override void OnEnd(LogRecord data)
    {
        var attributes = data.Attributes?.ToList() ?? [];

        try
        {
            // Collect attributes from all providers
            foreach (var provider in _attributeProviders)
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
                    _logger.LogError(ex, "Error getting attributes from provider {ProviderType}. Continuing with remaining providers.",
                        provider.GetType().Name);
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