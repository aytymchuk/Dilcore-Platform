using System.Diagnostics;
using Dilcore.Telemetry.Abstractions;
using Microsoft.Extensions.Logging;
using OpenTelemetry;

namespace Dilcore.Telemetry.Extensions.OpenTelemetry;

/// <summary>
/// Unified processor that enriches OpenTelemetry activities (traces) with attributes from all registered providers.
/// </summary>
public class UnifiedActivityProcessor : BaseProcessor<Activity>
{
    private readonly IEnumerable<ITelemetryAttributeProvider> _attributeProviders;
    private readonly ILogger<UnifiedActivityProcessor> _logger;

    public UnifiedActivityProcessor(
        IEnumerable<ITelemetryAttributeProvider> attributeProviders,
        ILogger<UnifiedActivityProcessor> logger)
    {
        _attributeProviders = attributeProviders;
        _logger = logger;
    }

    public override void OnEnd(Activity data)
    {
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
                        // SetTag will overwrite if key already exists
                        data.SetTag(attribute.Key, attribute.Value);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to get attributes from provider {ProviderName}", provider.GetType().Name);
                }
            }
        }
        finally
        {
            // Always call base.OnEnd even if enrichment fails
            base.OnEnd(data);
        }
    }
}