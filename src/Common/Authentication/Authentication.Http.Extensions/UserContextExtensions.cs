using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace Dilcore.Authentication.Http.Extensions;

/// <summary>
/// Extension methods for extracting user information from HttpContext.
/// </summary>
public static class UserContextExtensions
{
    /// <summary>
    /// Gets the stable user identifier from the NameIdentifier claim.
    /// </summary>
    /// <param name="httpContext">The HTTP context.</param>
    /// <returns>The user ID from NameIdentifier claim, or "anonymous" if not authenticated.</returns>
    public static string GetUserId(this HttpContext? httpContext)
    {
        return httpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "anonymous";
    }
}