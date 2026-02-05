using Dilcore.Identity.Actors.Profiles;
using Dilcore.Identity.Actors.Storage;
using Microsoft.Extensions.DependencyInjection;
using Orleans.Storage;

namespace Dilcore.Identity.Actors;

/// <summary>
/// Extension methods for configuring UserGrainStorage.
/// </summary>
public static class SiloBuilderExtensions
{
    /// <summary>
    /// Adds the UserGrainStorage provider to the silo.
    /// </summary>
    /// <param name="siloBuilder">The silo builder.</param>
    /// <returns>The silo builder.</returns>
    public static ISiloBuilder AddUserGrainStorage(this ISiloBuilder siloBuilder)
    {
        siloBuilder.ConfigureServices(services =>
        {
            services.AddAutoMapper(opt => opt.AddMaps(typeof(UserStateMappingProfile).Assembly));

            services.AddKeyedSingleton<IGrainStorage>("UserStore", (sp, key) =>
                ActivatorUtilities.CreateInstance<UserGrainStorage>(sp));
        });

        return siloBuilder;
    }
}
