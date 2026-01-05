using Dilcore.Authentication.Abstractions;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

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

        var userId = user.FindFirst(UserConstants.SubjectClaimType)?.Value
                     ?? user.FindFirst(UserConstants.UserIdClaimType)?.Value
                     ?? user.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        var email = user.FindFirst(UserConstants.EmailClaimType)?.Value
                    ?? user.FindFirst(ClaimTypes.Email)?.Value;

        var fullName = user.FindFirst(UserConstants.NameClaimType)?.Value
                       ?? user.FindFirst(ClaimTypes.Name)?.Value;

        if (userId == null)
            return null;

        return new UserContext(userId, email, fullName);
    }
}
