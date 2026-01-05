using System.Diagnostics;
using Dilcore.Telemetry.Abstractions;
using OpenTelemetry;

namespace Dilcore.WebApi.Extensions;

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
                    _logger.LogError(ex, "Error getting attributes from provider {ProviderType}. Continuing with remaining providers.",
                        provider.GetType().Name);
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
