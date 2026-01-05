using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;

namespace Dilcore.Authentication.Auth0;

/// <summary>
/// Extension methods for configuring Auth0 claims transformation.
/// </summary>
public static class Auth0Extensions
{
    /// <summary>
    /// Adds Auth0 claims transformation with user profile enrichment and caching.
    /// </summary>
    public static IServiceCollection AddAuth0ClaimsTransformation(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var auth0Settings = configuration.GetSection("AuthenticationSettings:Auth0").Get<Auth0Settings>()
            ?? throw new InvalidOperationException("Auth0 settings are not configured.");

        services.AddSingleton(auth0Settings);

        // Register typed HttpClient for Auth0UserService
        services.AddHttpClient<IAuth0UserService, Auth0UserService>(client =>
        {
            client.BaseAddress = new Uri($"https://{auth0Settings.Domain}/");
        })
        .SetHandlerLifetime(TimeSpan.FromMinutes(5)); // Reuse HttpMessageHandler for 5 minutes

        services.AddHttpContextAccessor();
        services.AddHybridCache();
        // Register as Scoped to match IAuth0UserService lifetime (typed HttpClient is scoped)
        services.AddScoped<IClaimsTransformation, Auth0ClaimsTransformation>();

        return services;
    }
}