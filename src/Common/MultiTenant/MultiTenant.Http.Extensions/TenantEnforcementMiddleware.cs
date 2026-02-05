using Dilcore.MultiTenant.Abstractions;
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
        // If endpoint is excluded or not found (404), skip enforcement
        if (context.IsExcludedFromMultiTenant())
        {
            await next(context);
            return;
        }

        // It will thrown TenantNotResolvedException and GlobalExceptionHandler return appropriate status code
        _tenantContextResolver.Resolve();

        await next(context);
    }
}