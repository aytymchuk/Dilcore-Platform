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

    public UnifiedLogRecordProcessor(IEnumerable<ITelemetryAttributeProvider> attributeProviders)
    {
        _attributeProviders = attributeProviders;
    }

    public override void OnEnd(LogRecord data)
    {
        var attributes = data.Attributes?.ToList() ?? [];

        // Collect attributes from all providers
        foreach (var provider in _attributeProviders)
        {
            var providerAttributes = provider.GetAttributes();
            foreach (var attribute in providerAttributes)
            {
                // Remove existing attribute with same key to avoid duplicates
                attributes.RemoveAll(kv => kv.Key == attribute.Key);
                attributes.Add(attribute);
            }
        }

        data.Attributes = attributes;

        base.OnEnd(data);
    }
}