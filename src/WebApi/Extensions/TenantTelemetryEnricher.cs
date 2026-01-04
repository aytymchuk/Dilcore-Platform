using System.Diagnostics;
using Dilcore.MultiTenant.Abstractions;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Dilcore.WebApi.Extensions;

public class TenantTelemetryEnricher : ITelemetryEnricher
{
    public void Enrich(Activity activity, HttpRequest request)
    {
        var tenantContextResolver = request.HttpContext.RequestServices.GetService<ITenantContextResolver>();
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