using Dilcore.WebApi.Client.Clients;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Extensions.Http;
using Refit;

namespace Dilcore.WebApi.Client;

/// <summary>
/// Extension methods for registering Platform API clients with dependency injection.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds the Platform API clients to the service collection.
    /// Registers IIdentityClient and ITenancyClient as independent services.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configureOptions">Action to configure client options.</param>
    /// <param name="configureClient">Optional action to configure all HTTP client builders (e.g., add delegating handlers).</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddPlatformApiClients(
        this IServiceCollection services,
        Action<PlatformApiClientOptions> configureOptions,
        Action<IHttpClientBuilder>? configureClient = null)
    {
        // Create options instance directly
        var options = new PlatformApiClientOptions();
        configureOptions(options);

        // Register all clients
        RegisterRefitClient<IIdentityClient>(services, options, configureClient);
        RegisterRefitClient<ITenancyClient>(services, options, configureClient);

        return services;
    }

    /// <summary>
    /// Registers a Refit client with common configuration.
    /// </summary>
    private static void RegisterRefitClient<TClient>(
        IServiceCollection services,
        PlatformApiClientOptions options,
        Action<IHttpClientBuilder>? configureClient)
        where TClient : class
    {
        var builder = services
            .AddRefitClient<TClient>()
            .ConfigureHttpClient(c =>
            {
                c.BaseAddress = options.BaseAddress;
                c.Timeout = options.Timeout;
            })
            .AddPolicyHandler(GetRetryPolicy(options));

        // Apply common configuration if provided
        configureClient?.Invoke(builder);
    }

    /// <summary>
    /// Creates a Polly retry policy for handling transient HTTP errors.
    /// </summary>
    private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy(PlatformApiClientOptions options)
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError() // Handles 5xx and 408
            .WaitAndRetryAsync(
                retryCount: options.RetryCount,
                sleepDurationProvider: retryAttempt =>
                    TimeSpan.FromSeconds(Math.Pow(options.RetryDelaySeconds, retryAttempt)));
    }
}
