using Dilcore.WebApi.Extensions;

using Dilcore.WebApi.Settings;

namespace Dilcore.WebApi.Infrastructure.OpenApi;

public static class OpenApiExtensions
{
    public static IServiceCollection AddOpenApiDocumentation(this IServiceCollection services, IConfiguration configuration)
    {
        var buildVersion = configuration[Constants.Configuration.BuildVersionKey] ?? Constants.Configuration.DefaultBuildVersion;
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
        });

        return services;
    }

    public static WebApplication UseOpenApiDocumentation(this WebApplication app)
    {
        app.MapOpenApi().AllowAnonymous();
        return app;
    }
}
