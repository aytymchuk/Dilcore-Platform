using System.Diagnostics;
using Dilcore.WebApi.Infrastructure.MultiTenant;
using OpenTelemetry;

namespace Dilcore.WebApi.Infrastructure.MultiTenant;

/// <summary>
/// Enriches OpenTelemetry activities (traces) with tenant context information.
/// </summary>
public class TenantActivityProcessor : BaseProcessor<Activity>
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public TenantActivityProcessor(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public override void OnEnd(Activity data)
    {
        var context = _httpContextAccessor.HttpContext;
        if (context == null)
        {
            base.OnEnd(data);
            return;
        }

        // Extract Tenant ID from ITenantContext
        var tenantContext = context.RequestServices?.GetService<ITenantContext>();
        if (tenantContext?.Name != null)
        {
            data.SetTag("tenant.id", tenantContext.Name);
        }

        base.OnEnd(data);
    }
}
