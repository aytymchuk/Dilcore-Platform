using Microsoft.AspNetCore.OpenApi;
using Dilcore.Extensions.OpenApi.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;

namespace Dilcore.Extensions.OpenApi;

public static class OpenApiExtensions
{
    public static IServiceCollection AddOpenApiDocumentation(this IServiceCollection services, Action<OpenApiSettings> configure)
    {
        var settings = new OpenApiSettings();
        configure(settings);

        if (string.IsNullOrWhiteSpace(settings.Name))
        {
            throw new InvalidOperationException("OpenApiSettings.Name is required for OpenAPI documentation.");
        }

        services.AddSingleton(settings);

        services.AddOpenApi(options =>
        {
            options.AddDocumentTransformer((document, context, cancellationToken) =>
            {
                document.Info = new()
                {
                    Title = settings.Name,
                    Version = settings.Version,
                    Description = settings.Description
                };
                return Task.CompletedTask;
            });

            // Add security schemes for authentication (Bearer + OAuth2)
            options.AddDocumentTransformer<OpenApiSecurityTransformer>();
            // Add global Problem Details responses
            options.AddDocumentTransformer<OpenApiProblemDetailsTransformer>();
            options.AddOperationTransformer<OpenApiProblemDetailsTransformer>();

            openApise
        });

        return services;
    }

    public static WebApplication UseOpenApiDocumentation(this WebApplication app, Action<IEndpointConventionBuilder>? configure = null)
    {
        var builder = app.MapOpenApi().AllowAnonymous();
        configure?.Invoke(builder);

        return app;
    }
}