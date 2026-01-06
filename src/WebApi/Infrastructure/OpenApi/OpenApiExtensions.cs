using Dilcore.WebApi.Settings;
using Finbuckle.MultiTenant.AspNetCore.Extensions;
using Dilcore.Configuration.Extensions;

namespace Dilcore.WebApi.Infrastructure.OpenApi;

public static class OpenApiExtensions
{
    public static IServiceCollection AddOpenApiDocumentation(this IServiceCollection services, IConfiguration configuration)
    {
        var buildVersion = configuration[Configuration.AspNetCore.Constants.BuildVersionKey] ?? Configuration.AspNetCore.Constants.DefaultBuildVersion;
        var appSettings = configuration.GetRequiredSettings<ApplicationSettings>();

        if (string.IsNullOrWhiteSpace(appSettings.Name))
        {
            throw new InvalidOperationException("ApplicationSettings.Name is required for OpenAPI documentation.");
        }

        var appName = appSettings.Name;

        services.AddOpenApi(options =>
        {
            options.AddDocumentTransformer((document, _, _) =>
            {
                document.Info.Title = appName;
                document.Info.Version = buildVersion;
                document.Servers = [];
                return Task.CompletedTask;
            });

            // Add security schemes for authentication (Bearer + OAuth2)
            options.AddDocumentTransformer<OpenApiSecurityTransformer>();
            // Add global Problem Details responses
            options.AddDocumentTransformer<OpenApiProblemDetailsTransformer>();
            options.AddOperationTransformer<OpenApiProblemDetailsTransformer>();
            // Add FluentValidation rules to schema
            options.AddSchemaTransformer<OpenApiValidationSchemaTransformer>();
            // Add tenant header parameter to multi-tenant endpoints
            options.AddOperationTransformer<OpenApiTenantHeaderTransformer>();
        });

        return services;
    }

    public static WebApplication UseOpenApiDocumentation(this WebApplication app)
    {
        app.MapOpenApi().AllowAnonymous().ExcludeFromMultiTenantResolution();
        return app;
    }
}