using Dilcore.Blueprints.Core.Abstractions;
using Dilcore.Blueprints.Store.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace Dilcore.Blueprints.Store;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddBlueprintsStore(this IServiceCollection services)
    {
        services.AddAutoMapper(opts => opts.AddMaps(typeof(ServiceCollectionExtensions).Assembly));
        services.AddSingleton<BlueprintsIndexInitializer>();
        services.AddScoped<IEntityDefinitionRepository, EntityDefinitionRepository>();

        return services;
    }
}
