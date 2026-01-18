using Microsoft.AspNetCore.OpenApi;

namespace Dilcore.CorrelationId.Extensions.OpenApi;

public static class CorrelationIdOpenApiExtensions
{
    /// <summary>
    /// Adds correlation ID OpenAPI support by registering the correlation ID header transformer.
    /// </summary>
    /// <param name="options">The OpenAPI options.</param>
    /// <returns>The options for chaining.</returns>
    public static OpenApiOptions AddCorrelationIdSupport(this OpenApiOptions options)
    {
        options.AddOperationTransformer<OpenApiCorrelationIdHeaderTransformer>();
        return options;
    }
}
