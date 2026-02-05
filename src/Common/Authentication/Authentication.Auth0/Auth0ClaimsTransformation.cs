using System.Security.Claims;
using Dilcore.Authentication.Abstractions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;

namespace Dilcore.Authentication.Auth0;

/// <summary>
/// Transforms claims by enriching them with user data from Auth0 if missing.
/// Implements caching to minimize Auth0 API calls.
/// </summary>
public sealed class Auth0ClaimsTransformation : IClaimsTransformation
{
    private readonly IAuth0UserService _auth0UserService;
    private readonly HybridCache _cache;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<Auth0ClaimsTransformation> _logger;
    private readonly TimeSpan _cacheExpiration;

    private const string BearerScheme = "Bearer";

    public Auth0ClaimsTransformation(
        IAuth0UserService auth0UserService,
        HybridCache cache,
        IHttpContextAccessor httpContextAccessor,
        ILogger<Auth0ClaimsTransformation> logger,
        Auth0Settings settings)
    {
        _auth0UserService = auth0UserService;
        _cache = cache;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
        _cacheExpiration = TimeSpan.FromMinutes(settings.UserProfileCacheMinutes);
    }

    public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        if (principal.Identity?.IsAuthenticated != true)
            return principal;

        var userId = principal.FindFirst(UserConstants.SubjectClaimType)?.Value
                     ?? principal.FindFirst(UserConstants.UserIdClaimType)?.Value
                     ?? principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (userId == null)
            return principal;

        var hasEmail = principal.FindFirst(UserConstants.EmailClaimType) != null
                       || principal.FindFirst(ClaimTypes.Email) != null;

        var hasName = principal.FindFirst(UserConstants.NameClaimType) != null
                      || principal.FindFirst(ClaimTypes.Name) != null;

        // If we already have both email and name, no need to enrich
        if (hasEmail && hasName)
            return principal;

        // Get access token from Authorization header (avoiding GetTokenAsync to prevent infinite recursion)
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null)
            return principal;

        var accessToken = GetAccessTokenFromHeader(httpContext);

        if (string.IsNullOrEmpty(accessToken))
        {
            _logger.LogNoAccessToken();
            return principal;
        }

        // Get or create cached profile using HybridCache
        var cacheKey = $"auth0_user_{userId}";
        var token = httpContext.RequestAborted;

        var profile = await _cache.GetOrCreateAsync(
            cacheKey,
            async cancel =>
            {
                // Fetch from Auth0
                _logger.LogAuth0ProfileCacheMiss(userId);
                var userProfile = await _auth0UserService.GetUserProfileAsync(accessToken, cancel);

                if (userProfile != null)
                {
                    _logger.LogAuth0ProfileFetched(userId);
                }

                return userProfile;
            },
            new HybridCacheEntryOptions
            {
                Expiration = _cacheExpiration,
                LocalCacheExpiration = _cacheExpiration
            },
            tags: null,
            cancellationToken: token);

        if (profile == null)
            return principal;

        _logger.LogAuth0ProfileCacheHit(userId);

        // Clone the principal to avoid modifying the original
        var currentHost = $"{httpContext.Request.Scheme}://{httpContext.Request.Host}";
        return AddMissingClaims(principal, profile, hasEmail, hasName, currentHost);
    }

    private static string? GetAccessTokenFromHeader(HttpContext httpContext)
    {
        // Robustly parse Authorization header
        var authHeader = httpContext.Request.Headers.Authorization
            .FirstOrDefault(h => h?.StartsWith(BearerScheme, StringComparison.OrdinalIgnoreCase) ?? false);

        if (string.IsNullOrWhiteSpace(authHeader))
            return null;

        var parts = authHeader.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);

        // Verify scheme is "Bearer" and token is present
        if (parts.Length > 1 &&
            parts[0].Equals(BearerScheme, StringComparison.OrdinalIgnoreCase) &&
            !string.IsNullOrWhiteSpace(parts[1]))
        {
            return parts[1].Trim();
        }

        return null;
    }

    private ClaimsPrincipal AddMissingClaims(
        ClaimsPrincipal principal,
        Auth0UserProfile profile,
        bool hasEmail,
        bool hasName,
        string issuer)
    {
        var clonedPrincipal = principal.Clone();
        var identity = (ClaimsIdentity)clonedPrincipal.Identity!;
        var claimsAdded = new List<string>();

        if (!hasEmail && !string.IsNullOrEmpty(profile.Email))
        {
            identity.AddClaim(new Claim(ClaimTypes.Email, profile.Email, ClaimValueTypes.String, issuer));
            claimsAdded.Add("email");
        }

        if (!hasName && !string.IsNullOrEmpty(profile.Name))
        {
            identity.AddClaim(new Claim(ClaimTypes.Name, profile.Name, ClaimValueTypes.String, issuer));
            claimsAdded.Add("name");
        }

        if (claimsAdded.Count > 0)
        {
            _logger.LogClaimsAdded(string.Join(", ", claimsAdded));
        }

        return clonedPrincipal;
    }
}
