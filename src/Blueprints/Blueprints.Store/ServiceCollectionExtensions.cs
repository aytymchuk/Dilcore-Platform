using Dilcore.Blueprints.Core.Abstractions;
using Dilcore.Blueprints.Store.Repositories;
using Dilcore.Configuration.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Dilcore.Blueprints.Store;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddBlueprintsStore(
        this IServiceCollection services, IConfiguration configuration)
    {
        var mongoDbSettings = configuration.GetRequiredSettings<MongoDbSettings>();
        services.AddBlueprintsMongoDb(mongoDbSettings);

        services.AddAutoMapper(opts => opts.AddMaps(typeof(ServiceCollectionExtensions).Assembly));
        services.AddScoped<IEntityDefinitionRepository, EntityDefinitionRepository>();

        return services;
    }
}
