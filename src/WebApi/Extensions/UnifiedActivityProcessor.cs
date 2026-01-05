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

    public UnifiedActivityProcessor(IEnumerable<ITelemetryAttributeProvider> attributeProviders)
    {
        _attributeProviders = attributeProviders;
    }

    public override void OnEnd(Activity data)
    {
        // Collect attributes from all providers
        foreach (var provider in _attributeProviders)
        {
            var providerAttributes = provider.GetAttributes();
            foreach (var attribute in providerAttributes)
            {
                // SetTag will overwrite if key already exists
                data.SetTag(attribute.Key, attribute.Value);
            }
        }

        base.OnEnd(data);
    }
}
