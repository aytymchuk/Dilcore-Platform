using Dilcore.Extensions.OpenApi.Abstractions;
using Finbuckle.MultiTenant.AspNetCore.Extensions;
using Microsoft.AspNetCore.Builder;
using Scalar.AspNetCore;

namespace Dilcore.Extensions.Scalar;

public static class ScalarExtensions
{
    public static WebApplication UseScalarDocumentation(this WebApplication app, Action<ScalarSettings>? configure = null)
    {
        var settings = new ScalarSettings();

        configure?.Invoke(settings);

        app.MapScalarApiReference(settings.Endpoint, options =>
        {
            options.Title = $"{settings.Title} {settings.Version}";
            options.Theme = settings.Theme;

            options
                // Prefer auth0 (OAuth2) by default
                .AddPreferredSecuritySchemes(settings.Authentication?.PreferredSecurityScheme ?? Constants.Security.Auth0SchemeName)
                // Configure OAuth2 authorization code flow for Auth0
                .AddAuthorizationCodeFlow(Constants.Security.Auth0SchemeName, flow =>
                {
                    if (settings.Authentication is null) return;

                    flow.ClientId = settings.Authentication.ClientId;
                    flow.ClientSecret = settings.Authentication.ClientSecret;
                    // For Auth0, 'openid' is required for ID tokens
                    flow.SelectedScopes = settings.Authentication.Scopes;
                    // Auth0 requires the audience parameter for token requests
                    if (!string.IsNullOrEmpty(settings.Authentication.Audience))
                    {
                        flow.AddQueryParameter(Constants.Security.AudienceParameter, settings.Authentication.Audience);
                    }
                })
                // Configure Bearer token authentication for manual token input
                .AddHttpAuthentication(Constants.Security.BearerSchemeName, auth =>
                {
                    auth.Token = string.Empty;
                });
        }).AllowAnonymous().ExcludeFromMultiTenantResolution();

        return app;
    }
}
