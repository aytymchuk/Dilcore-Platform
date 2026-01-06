using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Dilcore.Configuration.Extensions;

public static class ConfigurationExtensions
{
    public static IServiceCollection RegisterConfiguration<T>(this IServiceCollection services, IConfiguration configuration) where T : class
    {
        var section = configuration.GetSection(typeof(T).Name);
        services.Configure<T>(section);
        return services;
    }

    public static T GetSettings<T>(this IConfiguration configuration) where T : class, new()
    {
        return configuration.GetSection(typeof(T).Name).Get<T>() ?? new T();
    }

    public static T GetRequiredSettings<T>(this IConfiguration configuration) where T : class
    {
        var section = configuration.GetSection(typeof(T).Name);
        return section.Get<T>() ?? throw new InvalidOperationException($"Required configuration section '{typeof(T).Name}' is missing.");
    }

    public static string GetValueOrDefault(this IConfiguration configuration, string key, string defaultValue)
    {
        return configuration[key] ?? defaultValue;
    }
}