using System.Diagnostics;
using Dilcore.Telemetry.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenTelemetry;

namespace Dilcore.Telemetry.Extensions.OpenTelemetry;

/// <summary>
/// Unified processor that enriches OpenTelemetry activities (traces) with attributes from all registered providers.
/// </summary>
public class UnifiedActivityProcessor : BaseProcessor<Activity>
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<UnifiedActivityProcessor> _logger;

    public UnifiedActivityProcessor(
        IServiceProvider serviceProvider,
        ILogger<UnifiedActivityProcessor> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public override void OnStart(Activity data)
    {
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
            base.OnStart(data);
        }
    }
}