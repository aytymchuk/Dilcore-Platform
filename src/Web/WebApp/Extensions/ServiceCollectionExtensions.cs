using Auth0.AspNetCore.Authentication;
using Dilcore.Configuration.Extensions;
using Dilcore.Telemetry.Extensions.OpenTelemetry;
using Dilcore.WebApp.Settings;
using Dilcore.WebApp.Constants;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.HttpOverrides;
using MudBlazor.Services;

namespace Dilcore.WebApp.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddWebAppServices(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment)
    {
        services.AddRazorComponents()
            .AddInteractiveServerComponents();

        services.AddMudServices();

        services.AddObservability(configuration, environment);
        services.AddAuthenticationServices(configuration);
        services.AddForwardedHeaders();

        return services;
    }

    public static IServiceCollection AddObservability(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment)
    {
        services.AddTelemetry(configuration, environment);
        return services;
    }

    public static IServiceCollection AddForwardedHeaders(this IServiceCollection services)
    {
        services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
        });
        return services;
    }

    public static IServiceCollection AddAuthenticationServices(this IServiceCollection services, IConfiguration configuration)
    {
        var authSettings = configuration.GetRequiredSettings<AuthenticationSettings>();
        var auth0Settings = authSettings?.Auth0;

        if (auth0Settings is null)
        {
            return services;
        }

        services.AddAuth0WebAppAuthentication(options =>
        {
            options.Domain = auth0Settings.Domain;
            options.ClientId = auth0Settings.ClientId;
            options.ClientSecret = auth0Settings.ClientSecret;
            options.Scope = auth0Settings.Scope;

            options.OpenIdConnectEvents = new OpenIdConnectEvents
            {
                OnTokenValidated = context =>
                {
                    if (context.TokenEndpointResponse?.AccessToken is { } accessToken)
                    {
                        var identity = context.Principal?.Identity as System.Security.Claims.ClaimsIdentity;
                        identity?.AddClaim(new System.Security.Claims.Claim(AuthConstants.AccessTokenClaim, accessToken));
                    }
                    return Task.CompletedTask;
                }
            };
        }).WithAccessToken(options =>
        {
            options.Audience = auth0Settings.Audience;
            options.Scope = auth0Settings.Scope;

            options.UseRefreshTokens = true;

            options.Events = new Auth0WebAppWithAccessTokenEvents
            {
                OnMissingAccessToken = OnMissingToken,
                OnMissingRefreshToken = OnMissingToken
            };
            
            return;
            
            async Task OnMissingToken(HttpContext context)
            {
                await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                var authenticationProperties = new LoginAuthenticationPropertiesBuilder().WithRedirectUri("/").Build();
                await context.ChallengeAsync(Auth0Constants.AuthenticationScheme, authenticationProperties);
            }
        });

        return services;
    }
}