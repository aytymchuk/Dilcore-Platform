using System.Diagnostics;
using Dilcore.MultiTenant.Abstractions;
using Dilcore.MultiTenant.Abstractions.Exceptions;
using OpenTelemetry;

namespace Dilcore.MultiTenant.Http.Extensions;

/// <summary>
/// Processes tenant activity events and handles related tenant-scoped operations.
/// </summary>
public sealed class TenantActivityProcessor : BaseProcessor<Activity>
{
    private readonly ITenantContextResolver _tenantContextResolver;

    public TenantActivityProcessor(ITenantContextResolver tenantContextResolver)
    {
        _tenantContextResolver = tenantContextResolver;
    }

    public override void OnEnd(Activity data)
    {
        // Resolve tenant context using the resolver directly
        ITenantContext? tenantContext = null;
        try
        {
            tenantContext = _tenantContextResolver.Resolve();
        }
        catch (TenantNotResolvedException)
        {
            // Ignore if tenant is not resolved for tracing
        }
        var tenantId = tenantContext?.Name;
        if (!string.IsNullOrEmpty(tenantId))
        {
            data.SetTag("tenant.id", tenantId);
        }

        base.OnEnd(data);
    }
}
