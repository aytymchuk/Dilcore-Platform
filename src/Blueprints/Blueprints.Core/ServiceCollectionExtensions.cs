using Dilcore.Blueprints.Core.Features.EntityDefinitions.Behaviors;
using Dilcore.Blueprints.Core.Features.EntityDefinitions.Create;
using Dilcore.Blueprints.Core.Features.EntityDefinitions.Create.Behaviors;
using Dilcore.Blueprints.Core.Features.EntityDefinitions.Update;
using Dilcore.MediatR.Extensions;
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
            cfg.RegisterEntityDefinitionBehaviors();
        });

        services.AddAutoMapper(cfg =>
        {
            cfg.AddMaps(typeof(ServiceCollectionExtensions).Assembly);
        });

        return services;
    }

    private static void RegisterEntityDefinitionBehaviors(this MediatRServiceConfiguration cfg)
    {
        cfg.AddBehavior<ValidateFieldSchemaNamesBehavior<CreateEntityDefinitionCommand>>();
        cfg.AddBehavior<ValidateFieldSchemaNamesBehavior<UpdateEntityDefinitionCommand>>();
        cfg.AddBehavior<UniqueSchemaNameBehavior>();
        cfg.AddBehavior<ValidateExtendsEntityBehavior>();
    }
}
