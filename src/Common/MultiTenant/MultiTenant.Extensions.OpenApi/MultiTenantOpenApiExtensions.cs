using Dilcore.Extensions.OpenApi.Abstractions;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.Extensions.DependencyInjection;

namespace Dilcore.MultiTenant.Extensions.OpenApi;

public static class MultipTenantOpenApiExtensions
{
    public static IServiceCollection AddMultiTenantOpenApiSupport(this IServiceCollection services)
    {
        // Configure the default "v1" document to use the tenant header transformer
        services.Configure<OpenApiOptions>(Dilcore.Configuration.AspNetCore.Constants.DefaultBuildVersion, options =>
        {
            options.AddOperationTransformer<OpenApiTenantHeaderTransformer>();
        });
        return services;
    }
}