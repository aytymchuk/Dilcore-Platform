using System.Reflection;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Dilcore.MediatR.Extensions;

public static class MediatRServiceCollectionExtensions
{
    public static IServiceCollection AddMediatRInfrastructure(this IServiceCollection services, Assembly assembly, Action<MediatRServiceConfiguration>? configuration = null)
    {
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(assembly);

            // Add Behaviors
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(TracingBehavior<,>));
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));

            configuration?.Invoke(cfg);
        });

        services.AddValidatorsFromAssembly(assembly);

        return services;
    }
}