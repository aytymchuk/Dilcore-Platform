using Dilcore.MultiTenant.Abstractions;
using Finbuckle.MultiTenant.AspNetCore.Routing;
using Microsoft.AspNetCore.Http;

namespace Dilcore.MultiTenant.Http.Extensions;

/// <summary>
/// Middleware that enforces tenant resolution for endpoints that are not excluded from multi-tenancy.
/// </summary>
public class TenantEnforcementMiddleware : IMiddleware
{
    private readonly ITenantContextResolver _tenantContextResolver;

    public TenantEnforcementMiddleware(ITenantContextResolver tenantContextResolver)
    {
        _tenantContextResolver = tenantContextResolver;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var endpoint = context.GetEndpoint();

        // If endpoint is excluded or not found (404), skip enforcement
        if (endpoint == null || endpoint.Metadata.GetMetadata<IExcludeFromMultiTenantResolutionMetadata>() != null)
        {
            await next(context);
            return;
        }

        // Resolve tenant (throws if missing)
        _tenantContextResolver.Resolve();

        await next(context);
    }
}
