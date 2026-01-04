using Dilcore.WebApi.Extensions;
using Dilcore.WebApi.Settings;
using Scalar.AspNetCore;
using Finbuckle.MultiTenant.AspNetCore.Extensions;

namespace Dilcore.WebApi.Infrastructure.Scalar;

public static class ScalarExtensions
{
    public static WebApplication AddScalarDocumentation(this WebApplication app, IConfiguration configuration)
    {
        var appSettings = configuration.GetRequiredSettings<ApplicationSettings>();
        var authSettings = configuration.GetRequiredSettings<AuthenticationSettings>();

        var buildVersion = configuration[Constants.Configuration.BuildVersionKey] ?? Constants.Configuration.DefaultBuildVersion;
        var appName = appSettings.Name;

        app.MapScalarApiReference(Constants.Scalar.Endpoint, options =>
        {
            options.Title = $"{appName} {buildVersion}";
            options.Theme = ScalarTheme.DeepSpace;

            options
                // Prefer auth0 (OAuth2) by default
                .AddPreferredSecuritySchemes(Constants.Security.Auth0SchemeName)
                // Configure OAuth2 authorization code flow for Auth0
                .AddAuthorizationCodeFlow(Constants.Security.Auth0SchemeName, flow =>
                {
                    if (authSettings.Auth0 is null) return;

                    flow.ClientId = authSettings.Auth0.ClientId;
                    flow.ClientSecret = authSettings.Auth0.ClientSecret;
                    // For Auth0, 'openid' is required for ID tokens
                    flow.SelectedScopes = [Constants.Security.OpenIdScope];
                    // Auth0 requires the audience parameter for token requests
                    flow.AddQueryParameter(Constants.Security.AudienceParameter, authSettings.Auth0.Audience);
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
