using System.Diagnostics;
using Dilcore.MultiTenant.Abstractions;
using Dilcore.MultiTenant.Abstractions.Exceptions;
using Dilcore.Telemetry.Abstractions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Dilcore.MultiTenant.Http.Extensions.Telemetry;

public class TenantTelemetryEnricher : ITelemetryEnricher
{
    public void Enrich(Activity activity, HttpRequest request)
    {
        if (request.HttpContext.IsExcludedFromMultiTenant())
        {
            return;
        }

        var tenantContextResolver = request.HttpContext.RequestServices.GetService<ITenantContextResolver>();
        if (tenantContextResolver?.TryResolve(out var tenantContext) == true &&
            !string.IsNullOrEmpty(tenantContext?.Name))
        {
            activity.SetTag(TenantConstants.TelemetryTagName, tenantContext.Name);
        }
    }
}