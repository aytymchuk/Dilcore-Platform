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
public sealed class UserTenantAuthorizeMiddleware(RequestDelegate next, ILogger<UserTenantAuthorizeMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context, ITenantContextResolver tenantContextResolver)
    {
        if (context.IsExcludedFromMultiTenant())
        {
            await next(context);
            return;
        }

        // 1. Resolve tenant context
        // If resolution fails or returns empty/null context, we proceed but validation below will likely skip or fail depending on logic.
        // The original code checked: if (tenantContext.Id == Guid.Empty || string.IsNullOrEmpty(tenantContext.StorageIdentifier)) -> skip
        
        if (!tenantContextResolver.TryResolve(out var tenantContext))
        {
            // If we can't resolve a tenant, we can't enforce tenant authorization.
            // Depending on requirements this might be 404 or just proceed (public endpoint might be caught by IsExcluded, but what if it's not excluded but no tenant provided?)
            // Original behavior: if empty, await next(context).
            await next(context);
            return;
        }

        // 2. Skip if user is not authenticated (AuthenticationMiddleware handles 401)
        if (context.User.Identity?.IsAuthenticated != true)
        {
            await next(context);
            return;
        }

        // 3. Authorization Check
        // We check if the user has a claim "tenants" that matches the current tenant's StorageIdentifier
        // The "tenants" claim was added by UserClaimsTransformation
        var userTenants = context.User.FindAll(UserConstants.TenantsClaimType).Select(c => c.Value).ToHashSet();
        
        // Also support single claim with JSON or comma-separated list if implementation changes
        // But for now UserClaimsTransformation adds multiple claims.

        if (userTenants.Contains(tenantContext.Name, StringComparer.OrdinalIgnoreCase))
        {
            await next(context);
            return;
        }

        // 4. Forbidden - Authenticated but not Authorized for this tenant
        var availableTenants = string.Join(", ", userTenants);
        logger.LogTenantAccessForbidden(context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "unknown", tenantContext.Name ?? "unknown-tenant");
        
        throw new ForbiddenException($"Access to tenant '{tenantContext.Name}' is forbidden. User tenants: [{availableTenants}]");
    }
}
