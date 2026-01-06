using Auth0.AspNetCore.Authentication.Api;
using Dilcore.Authentication.Abstractions;
using Dilcore.Authentication.Auth0;
using Dilcore.Telemetry.Abstractions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Dilcore.Authentication.Http.Extensions;

public static class AuthenticationExtensions
{
    public static IServiceCollection AddAuth0Authentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var authSettings = configuration.GetSection("AuthenticationSettings:Auth0").Get<Auth0Settings>()
            ?? throw new InvalidOperationException(
                "Auth0 configuration is required. Please ensure AuthenticationSettings:Auth0 is configured in appsettings.json.");

        services.AddAuth0ApiAuthentication(options =>
        {
            options.Domain = authSettings.Domain;
            options.JwtBearerOptions = new JwtBearerOptions
            {
                Audience = authSettings.Audience,
                Events = ConfigureJwtBearerEvents()
            };
        });

        // Enforce authentication for all endpoints by default
        services.AddAuthorizationBuilder()
            .SetFallbackPolicy(new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build());

        // Register UserContext services
        services.AddHttpContextAccessor();
        services.AddScoped<IUserContextProvider, HttpUserContextProvider>();
        services.AddScoped<IUserContextResolver, UserContextResolver>();

        services.AddSingleton<ITelemetryAttributeProvider, UserAttributeProvider>();

        return services;
    }

    private static JwtBearerEvents ConfigureJwtBearerEvents()
    {
        return new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<JwtBearerEvents>>();
                logger.LogAuthenticationFailed(context.Exception, context.Request.Path, context.Exception.Message);
                return Task.CompletedTask;
            },
            OnChallenge = context =>
            {
                var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<JwtBearerEvents>>();
                if (context.AuthenticateFailure != null)
                {
                    logger.LogAuthenticationChallenge(
                        context.Request.Path,
                        context.Error ?? context.AuthenticateFailure.Message,
                        context.ErrorDescription ?? "No description");
                }
                return Task.CompletedTask;
            }
        };
    }
}

internal static partial class LoggerExtensions
{
    [LoggerMessage(
        EventId = 3001,
        Level = LogLevel.Warning,
        Message = "Authentication failed for {Path}: {ErrorMessage}")]
    public static partial void LogAuthenticationFailed(
        this ILogger logger,
        Exception exception,
        string path,
        string errorMessage);

    [LoggerMessage(
        EventId = 3002,
        Level = LogLevel.Warning,
        Message = "Authentication challenge for {Path}: Error={Error}, Description={Description}")]
    public static partial void LogAuthenticationChallenge(
        this ILogger logger,
        string path,
        string error,
        string description);
}