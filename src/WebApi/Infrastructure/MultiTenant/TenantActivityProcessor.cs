using System.Diagnostics;
using Dilcore.WebApi.Infrastructure.MultiTenant;
using OpenTelemetry;

namespace Dilcore.WebApi.Infrastructure.MultiTenant;

/// <summary>
/// Enriches OpenTelemetry activities (traces) with tenant context information.
/// </summary>
public class TenantActivityProcessor : BaseProcessor<Activity>
{
    private readonly IServiceProvider _serviceProvider;

    public TenantActivityProcessor(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public override void OnEnd(Activity data)
    {
        // Resolve tenant context (must resolve from scoped provider)
        var resolver = _serviceProvider.GetService<ITenantContextResolver>();
        var tenantContext = resolver?.Resolve();

        if (tenantContext?.Name != null)
        {
            data.SetTag("tenant.id", tenantContext.Name);
        }

        base.OnEnd(data);
    }
}
