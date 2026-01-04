using System.Diagnostics;
using Dilcore.MultiTenant.Abstractions;

using Dilcore.Telemetry.Abstractions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Dilcore.MultiTenant.Http.Extensions.Telemetry;

public class TenantTelemetryEnricher : ITelemetryEnricher
{
    public void Enrich(Activity activity, object request)
    {
        if (request is not HttpRequest httpRequest)
        {
            return;
        }

        if (httpRequest.HttpContext.IsExcludedFromMultiTenant())
        {
            return;
        }

        var tenantContextResolver = httpRequest.HttpContext.RequestServices.GetService<ITenantContextResolver>();
        try
        {
            var tenantContext = tenantContextResolver?.Resolve();
            if (!string.IsNullOrEmpty(tenantContext?.Name))
            {
                activity.SetTag("tenant.id", tenantContext.Name);
            }
        }
        catch (TenantNotResolvedException)
        {
            // Ignore if tenant is not resolved
        }
    }
}