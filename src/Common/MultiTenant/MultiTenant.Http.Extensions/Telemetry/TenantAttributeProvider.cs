using Dilcore.MultiTenant.Abstractions;
using Dilcore.Telemetry.Abstractions;

namespace Dilcore.MultiTenant.Http.Extensions.Telemetry;

/// <summary>
/// Provides tenant context attributes for OpenTelemetry telemetry.
/// </summary>
public class TenantAttributeProvider : ITelemetryAttributeProvider
{
    private readonly ITenantContextResolver _tenantContextResolver;

    public TenantAttributeProvider(ITenantContextResolver tenantContextResolver)
    {
        _tenantContextResolver = tenantContextResolver;
    }

    public IEnumerable<KeyValuePair<string, object?>> GetAttributes()
    {
        if (_tenantContextResolver.TryResolve(out var tenantContext) &&
            !string.IsNullOrEmpty(tenantContext?.Name))
        {
            yield return new KeyValuePair<string, object?>(TenantConstants.TelemetryTagName, tenantContext.Name);
        }
    }
}