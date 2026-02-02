using Dilcore.Identity.Core.Features.Register.Behaviors;
using Dilcore.MediatR.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace Dilcore.Identity.Core;

/// <summary>
/// Service collection extensions for Identity.Core dependency injection.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds Identity Core services including MediatR handlers and behaviors.
    /// </summary>
    public static IServiceCollection AddIdentityApplication(this IServiceCollection services)
    {
        // Register MediatR handlers from this assembly with behaviors (Logging, Tracing) and validators
        services.AddMediatRInfrastructure(typeof(ServiceCollectionExtensions).Assembly, cfg => 
        {
            // Register email validation behavior for RegisterUserCommand
            cfg.AddBehavior<ValidateUserEmailBehavior>();
        });

        return services;
    }
}