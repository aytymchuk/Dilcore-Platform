using Dilcore.MediatR.Extensions;
using Dilcore.Tenancy.Core.Features.Create.Behaviors;
using Microsoft.Extensions.DependencyInjection;

namespace Dilcore.Tenancy.Core;

/// <summary>
/// Service collection extensions for Tenancy.Core dependency injection.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds Tenancy Application services including MediatR handlers and behaviors.
    /// </summary>
    public static IServiceCollection AddTenancyApplication(this IServiceCollection services)
    {
        // Register MediatR handlers from this assembly with behaviors (Logging, Tracing) and validators
        services.AddMediatRInfrastructure(typeof(ServiceCollectionExtensions).Assembly, cfg =>
        {
            cfg.AddCreateTenantCommandBehaviors();
        });

        return services;
    }

    private static void AddCreateTenantCommandBehaviors(this MediatRServiceConfiguration cfg)
    {
        cfg.AddBehavior<VerifyUserRegisteredBehavior>();
    }
}
