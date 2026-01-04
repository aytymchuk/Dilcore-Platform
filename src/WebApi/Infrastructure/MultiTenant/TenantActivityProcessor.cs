using System.Diagnostics;
using Dilcore.WebApi.Infrastructure.MultiTenant;
using OpenTelemetry;

namespace Dilcore.WebApi.Infrastructure.MultiTenant;

/// <summary>solu
/// Enriches OpenTelemetry activities (traces) with tenant context information.
/// </summary>
public class TenantActivityProcessor : BaseProcessor<Activity>
{
    private readonly ITenantContextResolver _tenantContextResolver;

    public TenantActivityProcessor(ITenantContextResolver tenantContextResolver)
    {
        _tenantContextResolver = tenantContextResolver;
    }

    public override void OnEnd(Activity data)
    {
        // Resolve tenant context using the resolver directly
        var tenantContext = _tenantContextResolver.Resolve();
        var tenantId = tenantContext?.Name;
        if (!string.IsNullOrEmpty(tenantId))
        {
            data.SetTag("tenant.id", tenantId);
        }

        base.OnEnd(data);
    }
}
