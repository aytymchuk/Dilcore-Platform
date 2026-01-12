using Dilcore.MultiTenant.Abstractions;
using Dilcore.MultiTenant.Http.Extensions.Telemetry;
using Dilcore.Telemetry.Abstractions;
using Finbuckle.MultiTenant;
using Finbuckle.MultiTenant.Abstractions;
using Finbuckle.MultiTenant.AspNetCore.Extensions;
using Finbuckle.MultiTenant.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Dilcore.MultiTenant.Http.Extensions;

public static class MultiTenantExtensions
{
    /// <summary>
    /// Adds multi-tenancy services with default in-memory store.
    /// Use this for testing scenarios or when no custom store is needed.
    /// </summary>
    public static IServiceCollection AddMultiTenancy(this IServiceCollection services)
    {
        return services.AddMultiTenancy<TenantInfo>(_ => { });
    }

    /// <summary>
    /// Adds multi-tenancy services with default in-memory store for a specific tenant type.
    /// </summary>
    public static IServiceCollection AddMultiTenancy<TTenantInfo>(this IServiceCollection services)
        where TTenantInfo : TenantInfo
    {
        return services.AddMultiTenancy<TTenantInfo>(_ => { });
    }

    public static IServiceCollection AddMultiTenancy<TTenantInfo>(this IServiceCollection services, Action<MultiTenantBuilder<TTenantInfo>> builder)
        where TTenantInfo : TenantInfo
    {
        // 1. Configure Finbuckle
        var multiTenant = services.AddMultiTenant<TTenantInfo>()
            .WithHeaderStrategy(TenantConstants.HeaderName);

        builder.Invoke(multiTenant);

        // 2. Register providers
        services.AddTenantContextProvider<HttpTenantContextProvider>();
        services.AddTelemetryAttributeProvider<TenantAttributeProvider>();

        // 3. Register resolver
        services.AddSingleton<ITenantContextResolver, TenantContextResolver>();


        // 4. Register ITenantContext factory (resolves lazily via resolver)
        services.AddScoped<ITenantContext>(sp => sp.GetRequiredService<ITenantContextResolver>().Resolve());

        // 5. Register middleware
        services.AddScoped<TenantEnforcementMiddleware>();

        return services;
    }

    public static IApplicationBuilder UseMultiTenantEnforcement(this IApplicationBuilder app)
    {
        return app.UseMiddleware<TenantEnforcementMiddleware>();
    }
}