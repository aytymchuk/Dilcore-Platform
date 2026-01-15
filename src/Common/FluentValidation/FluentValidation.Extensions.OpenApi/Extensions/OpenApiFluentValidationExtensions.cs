using Microsoft.AspNetCore.OpenApi;
using Microsoft.Extensions.DependencyInjection;
using Dilcore.Extensions.OpenApi.Abstractions;

namespace Dilcore.FluentValidation.Extensions.OpenApi.Extensions;

/// <summary>
/// Extension methods for adding FluentValidation to OpenApi.
/// </summary>
public static class OpenApiFluentValidationExtensions
{
    /// <summary>
    /// Adds FluentValidation rules to the OpenApi schema.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <returns>The builder.</returns>
    public static OpenApiOptions AddFluentValidation(this OpenApiOptions options)
    {
        options.AddSchemaTransformer<OpenApiValidationSchemaTransformer>();
        return options;
    }
}
