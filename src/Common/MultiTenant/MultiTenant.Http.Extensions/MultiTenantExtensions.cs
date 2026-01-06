using Dilcore.MultiTenant.Abstractions;
using Dilcore.MultiTenant.Http.Extensions.Telemetry;
using Dilcore.Telemetry.Abstractions;
using Finbuckle.MultiTenant.AspNetCore.Extensions;
using Finbuckle.MultiTenant.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Dilcore.MultiTenant.Http.Extensions;

public static class MultiTenantExtensions
{
    public static IServiceCollection AddMultiTenancy(this IServiceCollection services)
    {
        // 1. Configure Finbuckle
        services.AddMultiTenant<AppTenantInfo>()
            .WithHeaderStrategy(TenantConstants.HeaderName)
            .WithInMemoryStore(options =>
            {
                options.Tenants.Add(new AppTenantInfo("t1", "t1", "T1")
                {
                    StorageIdentifier = "db-shard-01"
                });

                options.Tenants.Add(new AppTenantInfo("t2", "t2", "T2")
                {
                    StorageIdentifier = "db-shard-02"
                });
            });

        // 2. Register providers
        services.AddSingleton<ITenantContextProvider, HttpTenantContextProvider>();
        services.AddSingleton<ITelemetryAttributeProvider, TenantAttributeProvider>();

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
