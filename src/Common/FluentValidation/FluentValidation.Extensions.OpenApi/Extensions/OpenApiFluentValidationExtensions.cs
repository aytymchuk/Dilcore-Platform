using Microsoft.AspNetCore.OpenApi;
using Microsoft.Extensions.DependencyInjection;

namespace Dilcore.FluentValidation.Extensions.OpenApi.Extensions;

/// <summary>
/// Extension methods for adding FluentValidation to OpenApi.
/// </summary>
public static class OpenApiFluentValidationExtensions
{
    /// <summary>
    /// Adds FluentValidation rules to the OpenApi schema.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddFluentValidation(this IServiceCollection services)
    {
        services.ConfigureAll<OpenApiOptions>(options =>
        {
            options.AddSchemaTransformer<OpenApiValidationSchemaTransformer>();
        });
        return services;
    }
}
