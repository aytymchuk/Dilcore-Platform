using Dilcore.Extensions.OpenApi.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;

namespace Dilcore.Extensions.OpenApi;

public static class OpenApiExtensions
{
    public static IDilcoreOpenApiBuilder AddOpenApiDocumentation(this IServiceCollection services, Action<DilcoreOpenApiOptions> configure)
    {
        var dilcoreOptions = new DilcoreOpenApiOptions();
        configure(dilcoreOptions);

        if (string.IsNullOrWhiteSpace(dilcoreOptions.Settings.Name))
        {
            throw new InvalidOperationException("OpenApiSettings.Name is required for OpenAPI documentation.");
        }

        services.AddOpenApi(options =>
        {
            options.AddDocumentTransformer((document, _, _) =>
            {
                document.Info.Title = dilcoreOptions.Settings.Name;
                document.Info.Version = dilcoreOptions.Settings.Version;
                document.Servers = [];

                dilcoreOptions.ConfigureDocument?.Invoke(document);

                return Task.CompletedTask;
            });

            // Add security schemes for authentication (Bearer + OAuth2)
            options.AddDocumentTransformer<OpenApiSecurityTransformer>();
            // Add global Problem Details responses
            options.AddDocumentTransformer<OpenApiProblemDetailsTransformer>();
            options.AddOperationTransformer<OpenApiProblemDetailsTransformer>();

            dilcoreOptions.ConfigureOptions?.Invoke(options);
        });

        // Register the settings so they can be injected if needed
        services.AddSingleton(dilcoreOptions.Settings);

        return new DilcoreOpenApiBuilder(services);
    }

    public static WebApplication UseOpenApiDocumentation(this WebApplication app, Action<IEndpointConventionBuilder>? configure = null)
    {
        var builder = app.MapOpenApi().AllowAnonymous();
        configure?.Invoke(builder);
        
        return app;
    }
}