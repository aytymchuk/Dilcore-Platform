using Dilcore.Configuration.Extensions;
using Dilcore.Tenancy.Contracts.Tenants;
using Dilcore.Tenancy.Store.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Dilcore.Tenancy.Store;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddTenancyStore(this IServiceCollection services, IConfiguration configuration)
    {
        var mongoDbSettings = configuration.GetRequiredSettings<MongoDbSettings>();
        
        services.AddTenancyMongoDb(mongoDbSettings);
        services.AddScoped<ITenantRepository, TenantRepository>();
        return services;
    }
}
