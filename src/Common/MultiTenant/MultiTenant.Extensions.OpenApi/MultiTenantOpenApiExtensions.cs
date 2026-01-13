using Dilcore.Extensions.OpenApi.Abstractions;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.Extensions.DependencyInjection;

namespace Dilcore.MultiTenant.Extensions.OpenApi;

public static class MultipTenantOpenApiExtensions
{
    public static IDilcoreOpenApiBuilder AddMultiTenantOpenApiSupport(this IDilcoreOpenApiBuilder builder)
    {
        // Configure the default "v1" document to use the tenant header transformer
        builder.Services.Configure<OpenApiOptions>(Dilcore.Configuration.AspNetCore.Constants.DefaultBuildVersion, options =>
        {
            options.AddOperationTransformer<OpenApiTenantHeaderTransformer>();
        });
        return builder;
    }
}
