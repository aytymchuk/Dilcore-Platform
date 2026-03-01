using Dilcore.Blueprints.Core.Features.EntityDefinitions.Create.Behaviors;
using Dilcore.MediatR.Extensions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Dilcore.Blueprints.Core;

/// <summary>
/// Service collection extensions for Blueprints.Core dependency injection.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds Blueprints Application services including MediatR handlers and AutoMapper profiles.
    /// </summary>
    public static IServiceCollection AddBlueprintsApplication(this IServiceCollection services)
    {
        services.AddMediatRInfrastructure(typeof(ServiceCollectionExtensions).Assembly, cfg =>
        {
            cfg.RegisterCreateEntityDefinitionBehaviors();
        });

        services.AddAutoMapper(cfg =>
        {
            cfg.AddMaps(typeof(ServiceCollectionExtensions).Assembly);
        });

        return services;
    }

    private static void RegisterCreateEntityDefinitionBehaviors(this MediatRServiceConfiguration cfg)
    {
        cfg.AddBehavior<UniqueSchemaNameBehavior>();
    }
}
