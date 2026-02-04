using System.Security.Claims;
using Dilcore.Authentication.Abstractions;
using Microsoft.AspNetCore.Http;

namespace Dilcore.Authentication.Http.Extensions;

/// <summary>
/// Resolves user context from HTTP context using ClaimsPrincipal.
/// </summary>
public sealed class HttpUserContextProvider(IHttpContextAccessor httpContextAccessor) : IUserContextProvider
{
    public int Priority => 100;

    public IUserContext? GetUserContext()
    {
        var user = httpContextAccessor.HttpContext?.User;

        if (user?.Identity?.IsAuthenticated != true)
            return null;

        var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value
                     ?? user.FindFirst(UserConstants.SubjectClaimType)?.Value
                     ?? user.FindFirst(UserConstants.UserIdClaimType)?.Value;

        var email = user.FindFirst(ClaimTypes.Email)?.Value
                    ?? user.FindFirst(UserConstants.EmailClaimType)?.Value;

        var fullName = user.FindFirst(ClaimTypes.Name)?.Value
                       ?? user.FindFirst(UserConstants.NameClaimType)?.Value;

        if (userId == null)
            return null;


        var tenants = user.FindAll(UserConstants.TenantsClaimType).Select(c => c.Value);
        var roles = user.FindAll(ClaimTypes.Role).Select(c => c.Value);

        return new UserContext(userId, email, fullName, tenants, roles);
    }
}