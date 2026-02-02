using Dilcore.Configuration.Extensions;
using Dilcore.CorrelationId.Http.Extensions;
using Dilcore.Extensions.OpenApi;
using Dilcore.Extensions.OpenApi.Abstractions;
using Dilcore.Extensions.Scalar;
using Dilcore.MultiTenant.Http.Extensions;
using Dilcore.WebApi.Settings;
using Finbuckle.MultiTenant.AspNetCore.Extensions;

namespace Dilcore.WebApi.Extensions;

public static class MiddlewarePipelineExtensions
{
    public static WebApplication UseApplicationMiddleware(this WebApplication app)
    {
        app.UseExceptionHandler();
        app.UseStatusCodePages();

        // Configure OpenAPI documentation in development
        if (app.Environment.IsDevelopment())
        {
            app.UseApiDocumentation();
        }

        app.UseHttpsRedirection();
        app.UseCorrelationId();
        app.UseMultiTenant();

        app.UseAuthentication();
        app.UseMultiTenantEnforcement();
        app.UseAuthorization();

        return app;
    }

    private static void UseApiDocumentation(this WebApplication app)
    {
        app.UseOpenApiDocumentation(configure =>
        {
            configure.ExcludeFromMultiTenantResolution();
        });

        app.UseScalarDocumentation(scalar =>
        {
            // Map OpenAPI settings
            var openApiSettings = app.Services.GetService<OpenApiSettings>();
            if (openApiSettings != null)
            {
                scalar.Title = openApiSettings.Name;
                scalar.Version = openApiSettings.Version;
            }

            // Map Authentication settings
            var authSettings = app.Configuration.GetRequiredSettings<AuthenticationSettings>();
            if (authSettings.Auth0 != null)
            {
                scalar.Authentication = new ScalarAuthenticationSettings
                {
                    ClientId = authSettings.Auth0.ClientId,
                    ClientSecret = authSettings.Auth0.ClientSecret,
                    Audience = authSettings.Auth0.Audience,
                    Scopes = new HashSet<string>(authSettings.Scopes ?? ["openid", "profile", "email"])
                };
            }
        });
    }
}