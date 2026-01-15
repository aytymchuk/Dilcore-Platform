using Microsoft.AspNetCore.OpenApi;

namespace Dilcore.FluentValidation.Extensions.OpenApi.Extensions;

/// <summary>
/// Extension methods for adding FluentValidation to OpenApi.
/// </summary>
public static class OpenApiFluentValidationExtensions
{
    /// <summary>
    /// Adds FluentValidation rules to the OpenApi schema.
    /// </summary>
    /// <param name="options">The OpenApi options.</param>
    /// <returns>The options for chaining.</returns>
    public static OpenApiOptions AddFluentValidation(this OpenApiOptions options)
    {
        options.AddSchemaTransformer<OpenApiValidationSchemaTransformer>();
        return options;
    }
}