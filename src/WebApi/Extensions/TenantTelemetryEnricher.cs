using System.Diagnostics;
using Dilcore.MultiTenant.Abstractions;
using Dilcore.MultiTenant.Abstractions.Exceptions;
using Dilcore.Telemetry.Abstractions;

namespace Dilcore.WebApi.Extensions;

public class TenantTelemetryEnricher : ITelemetryEnricher
{
    public void Enrich(Activity activity, HttpRequest request)
    {
        var tenantContextResolver = request.HttpContext.RequestServices.GetService<ITenantContextResolver>();
        if (tenantContextResolver?.TryResolve(out var tenantContext) == true &&
            !string.IsNullOrEmpty(tenantContext?.Name))
        {
            activity.SetTag("tenant.id", tenantContext.Name);
        }
    }
}