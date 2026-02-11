using System.Security.Claims;
using Dilcore.Authentication.Abstractions;
using Dilcore.Authentication.Abstractions.Exceptions;
using Dilcore.MultiTenant.Abstractions;
using Dilcore.MultiTenant.Http.Extensions;
using Dilcore.WebApi.Extensions;

namespace Dilcore.WebApi.Middleware;

/// <summary>
/// Middleware that enforces tenant access authorization.
/// Verifies that the authenticated user belongs to the requested tenant.
/// </summary>
public sealed class UserTenantAuthorizeMiddleware : IMiddleware
{
    private readonly ILogger<UserTenantAuthorizeMiddleware> _logger;
    private readonly ITenantContextResolver _tenantContextResolver;

    public UserTenantAuthorizeMiddleware(
        ILogger<UserTenantAuthorizeMiddleware> logger,
        ITenantContextResolver tenantContextResolver)
    {
        _logger = logger;
        _tenantContextResolver = tenantContextResolver;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        if (context.IsExcludedFromMultiTenant())
        {
            await next(context);
            return;
        }

        if (!_tenantContextResolver.TryResolve(out var tenantContext))
        {
            await next(context);
            return;
        }

        if (context.User.Identity?.IsAuthenticated != true)
        {
            await next(context);
            return;
        }

        var userTenants = context.User.FindAll(UserConstants.TenantsClaimType).Select(c => c.Value).ToHashSet(StringComparer.OrdinalIgnoreCase);

        if (!string.IsNullOrWhiteSpace(tenantContext.Name) && userTenants.Contains(tenantContext.Name))
        {
            await next(context);
            return;
        }

        _logger.LogTenantAccessForbidden(context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "unknown", tenantContext.Name ?? "unknown");

        throw new ForbiddenException("Access to tenant is forbidden.");
    }
}
