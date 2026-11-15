using Dilcore.Tenancy.Core.Abstractions;
using Dilcore.Tenancy.Store.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace Dilcore.Tenancy.Store;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddTenancyStore(this IServiceCollection services)
    {
        services.AddHostedService<TenantCollectionIndexInitializer>();
        services.AddScoped<ITenantRepository, TenantRepository>();
        return services;
    }
}
