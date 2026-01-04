using Dilcore.WebApi.Infrastructure.MultiTenant;
using Finbuckle.MultiTenant.Abstractions;
using Finbuckle.MultiTenant.AspNetCore.Extensions;
using Finbuckle.MultiTenant.Extensions;

namespace Dilcore.WebApi.Extensions;

public static class MultiTenantExtensions
{
    public static IServiceCollection AddMultiTenancy(this IServiceCollection services)
    {
        // 1. Configure Finbuckle
        services.AddMultiTenant<AppTenantInfo>()
            .WithHeaderStrategy(Constants.Headers.Tenant)
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

        // 3. Register resolver
        services.AddSingleton<ITenantContextResolver, TenantContextResolver>();

        // 4. Register ITenantContext factory (resolves lazily via resolver)
        services.AddScoped<ITenantContext>(sp => sp.GetRequiredService<ITenantContextResolver>().Resolve());

        return services;
    }
}
