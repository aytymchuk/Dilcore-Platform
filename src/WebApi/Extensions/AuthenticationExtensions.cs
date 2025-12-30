using Auth0.AspNetCore.Authentication.Api;
using Dilcore.WebApi.Settings;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;

namespace Dilcore.WebApi.Extensions;

public static class AuthenticationExtensions
{
    public static IServiceCollection AddAuth0Authentication(this IServiceCollection services, IConfiguration configuration)
    {
        var authSettings = configuration.GetSettings<AuthenticationSettings>();

        if (authSettings.Auth0 == null)
        {
            throw new InvalidOperationException(
                "Auth0 configuration is required. Please ensure AuthenticationSettings.Auth0 is configured in appsettings.json.");
        }

        var auth0 = authSettings.Auth0;

        services.AddAuth0ApiAuthentication(options =>
        {
            options.Domain = auth0.Domain;
            options.JwtBearerOptions = new JwtBearerOptions
            {
                Audience = auth0.Audience,
                Events = ConfigureJwtBearerEvents()
            };
        });

        // Enforce authentication for all endpoints by default
        services.AddAuthorizationBuilder()
            .SetFallbackPolicy(new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build());

        return services;
    }

    private static JwtBearerEvents ConfigureJwtBearerEvents()
    {
        return new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<JwtBearerEvents>>();
                logger.LogWarning(
                    context.Exception,
                    "Authentication failed for request {Path}: {Message}",
                    context.Request.Path,
                    context.Exception.Message);
                return Task.CompletedTask;
            },
            OnChallenge = context =>
            {
                var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<JwtBearerEvents>>();
                if (context.AuthenticateFailure != null)
                {
                    logger.LogWarning(
                        "Authentication challenge issued for request {Path}: {Error} - {ErrorDescription}",
                        context.Request.Path,
                        context.Error ?? "Unknown",
                        context.ErrorDescription ?? "No description");
                }
                return Task.CompletedTask;
            }
        };
    }
}