using Dilcore.Blueprints.Actors.Profiles;
using Dilcore.Blueprints.Actors.Storage;
using Microsoft.Extensions.DependencyInjection;
using Orleans.Storage;

namespace Dilcore.Blueprints.Actors;

public static class SiloBuilderExtensions
{
    public const string StoreName = "BlueprintDefinitionStore";

    public static ISiloBuilder AddBlueprintsActors(this ISiloBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            services.AddAutoMapper(opt => opt.AddMaps(typeof(EntityDefinitionStateMappingProfile).Assembly));

            services.AddKeyedSingleton<IGrainStorage>(StoreName, (sp, _) =>
                ActivatorUtilities.CreateInstance<BlueprintDefinitionStorage>(sp));
        });

        return builder;
    }
}
