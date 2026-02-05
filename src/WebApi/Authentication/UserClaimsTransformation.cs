using System.Security.Claims;
using Dilcore.Authentication.Abstractions;
using Dilcore.Identity.Actors.Abstractions;
using Dilcore.MultiTenant.Abstractions;
using Microsoft.AspNetCore.Authentication;
using Dilcore.WebApi.Extensions;

namespace Dilcore.WebApi.Authentication;

/// <summary>
/// Transforms claims by enriching them with tenant and role information from the user grain.
/// Replaces the default Auth0 implementation to support multi-tenancy.
/// </summary>
public class UserClaimsTransformation(
    IClusterClient clusterClient,
    ITenantContextResolver tenantContextResolver,
    ILogger<UserClaimsTransformation> logger)
    : IClaimsTransformation
{
    private const string TransformationMarkerClaim = "urn:dilcore:claims-transformed";

    public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        // Skip if not authenticated or no subject/user ID
        if (principal.Identity?.IsAuthenticated != true)
        {
            return principal;
        }

        var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value
                     ?? principal.FindFirst(UserConstants.SubjectClaimType)?.Value
                     ?? principal.FindFirst(UserConstants.UserIdClaimType)?.Value;

        if (string.IsNullOrEmpty(userId))
        {
            return principal;
        }

        // Avoid infinite recursion by checking a dedicated marker claim
        if (principal.HasClaim(c => c.Type == TransformationMarkerClaim))
        {
            return principal;
        }

        try
        {
            // Clone the principal to avoid modifying the original one which might be cached
            var clonedPrincipal = principal.Clone();
            var clonedIdentity = clonedPrincipal.Identity as ClaimsIdentity;

            if (clonedIdentity == null)
            {
                return principal;
            }

            // Mark that we have processed this principal
            clonedIdentity.AddClaim(new Claim(TransformationMarkerClaim, "true"));
            
            var userGrain = clusterClient.GetGrain<IUserGrain>(userId);
            
            // Get user profile and tenants
            var userProfile = await userGrain.GetProfileAsync();
            var tenantAccessList = await userGrain.GetTenantsAsync();
            var tenantIds = tenantAccessList.Select(t => t.TenantId).ToHashSet();

            // Add user profile claims if available
            AddProfileClaims(clonedIdentity, userProfile);

            // Add tenants claim
            AddTenantClaims(clonedIdentity, tenantIds);

            // If we have a current tenant context, add roles for THAT tenant
            AddTenantRoleClaims(clonedIdentity, userId, tenantAccessList, tenantContextResolver);
            
            logger.LogTenantsAdded(userId, tenantIds.Count);

            return clonedPrincipal;
        }
        catch (Exception ex)
        {
            logger.LogTransformationError(ex, userId);
            // Return original principal on error to allow request to proceed (potentially unauthorized)
            // rather than crushing the request pipeline
            return principal;
        }
    }

    private static void AddProfileClaims(ClaimsIdentity identity, UserResponse? userProfile)
    {
        if (userProfile == null)
        {
            return;
        }

        if (!string.IsNullOrEmpty(userProfile.Email) && !identity.HasClaim(c => c.Type == ClaimTypes.Email))
        {
            // Add standard email claim
            identity.AddClaim(new Claim(ClaimTypes.Email, userProfile.Email));
        }

        if (!string.IsNullOrEmpty(userProfile.FirstName) || !string.IsNullOrEmpty(userProfile.LastName))
        {
            var name = $"{userProfile.FirstName} {userProfile.LastName}".Trim();
            if (!string.IsNullOrEmpty(name) && !identity.HasClaim(c => c.Type == ClaimTypes.Name))
            {
                // Add standard name claim
                identity.AddClaim(new Claim(ClaimTypes.Name, name));
            }
        }
    }

    private static void AddTenantClaims(ClaimsIdentity identity, IEnumerable<string> tenantIds)
    {
        foreach (var tenantId in tenantIds)
        {
            if (!identity.HasClaim(c => c.Type == UserConstants.TenantsClaimType && c.Value == tenantId))
            {
                identity.AddClaim(new Claim(UserConstants.TenantsClaimType, tenantId));
            }
        }
    }

    private void AddTenantRoleClaims(
        ClaimsIdentity identity,
        string userId,
        IReadOnlyList<TenantAccess> tenantAccessList,
        ITenantContextResolver tenantContextResolver)
    {
        if (tenantContextResolver.TryResolve(out var tenantContext) &&
            tenantContext != null &&
            tenantContext.Id != Guid.Empty &&
            !string.IsNullOrEmpty(tenantContext.Name))
        {
            // Find access record for current tenant
            var currentTenantAccess = tenantAccessList
                .FirstOrDefault(t => t.TenantId.Equals(tenantContext.Name, StringComparison.OrdinalIgnoreCase));

            if (currentTenantAccess != null)
            {
                foreach (var role in currentTenantAccess.Roles)
                {
                    identity.AddClaim(new Claim(ClaimTypes.Role, role));
                }

                logger.LogRolesAdded(userId, tenantContext.Name ?? "unknown", currentTenantAccess.Roles.Count);
            }
        }
    }
}
