using Dilcore.Tenancy.Actors.Profiles;
using Dilcore.Tenancy.Actors.Storage;
using Microsoft.Extensions.DependencyInjection;
using Orleans.Storage;

namespace Dilcore.Tenancy.Actors;

public static class SiloBuilderExtensions
{
    public static ISiloBuilder AddTenancyActors(this ISiloBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            services.AddAutoMapper(opt => opt.AddMaps(typeof(TenantStateMappingProfile).Assembly));

            services.AddKeyedSingleton<IGrainStorage>("TenantStore", (sp, key) =>
                ActivatorUtilities.CreateInstance<TenantGrainStorage>(sp));
        });

        return builder;
    }
}
