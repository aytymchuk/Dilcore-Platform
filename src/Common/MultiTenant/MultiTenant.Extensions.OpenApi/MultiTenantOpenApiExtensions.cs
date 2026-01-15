using Microsoft.AspNetCore.OpenApi;

namespace Dilcore.MultiTenant.Extensions.OpenApi;

public static class MultiTenantOpenApiExtensions
{
    /// <summary>
    /// Adds multi-tenant OpenAPI support by registering the tenant header transformer.
    /// </summary>
    /// <param name="options">The OpenAPI options.</param>
    /// <returns>The options for chaining.</returns>
    public static OpenApiOptions AddMultiTenantSupport(this OpenApiOptions options)
    {
        options.AddOperationTransformer<OpenApiTenantHeaderTransformer>();
        return options;
    }
}