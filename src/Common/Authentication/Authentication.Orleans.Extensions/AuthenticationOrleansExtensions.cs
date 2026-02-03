using Dilcore.Authentication.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Dilcore.Authentication.Orleans.Extensions;

/// <summary>
/// Extension methods for integrating authentication with Orleans.
/// </summary>
public static class AuthenticationOrleansExtensions
{
    /// <summary>
    /// Adds Orleans user context support to the silo.
    /// This enables user context propagation across grain calls.
    /// </summary>
    /// <param name="builder">The silo builder.</param>
    /// <returns>The silo builder for chaining.</returns>
    public static ISiloBuilder AddOrleansUserContext(this ISiloBuilder builder)
    {
        // Register the Orleans user context provider
        builder.ConfigureServices(services =>
        {
            // Register user context provider (singleton, higher priority than HTTP provider)
            services.AddUserContextProvider<OrleansUserContextProvider>();

            // Register IUserContext to resolve from the accessor from within Grains
            services.AddScoped<IUserContext>(sp =>
            {
                var resolver = sp.GetRequiredService<IUserContextResolver>();
                return resolver.TryResolve(out var userContext) ? userContext! : UserContext.Empty;
            });
        });

        // Register incoming grain call filter to extract user context
        builder.AddIncomingGrainCallFilter<UserIncomingGrainCallFilter>();

        // Register outgoing grain call filter to propagate user context
        builder.AddOutgoingGrainCallFilter<UserOutgoingGrainCallFilter>();

        return builder;
    }

    /// <summary>
    /// Registers a user context provider as a singleton.
    /// </summary>
    /// <typeparam name="TProvider">The type of the user context provider to register.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddUserContextProvider<TProvider>(this IServiceCollection services)
        where TProvider : class, IUserContextProvider
    {
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IUserContextProvider, TProvider>());
        return services;
    }
}
