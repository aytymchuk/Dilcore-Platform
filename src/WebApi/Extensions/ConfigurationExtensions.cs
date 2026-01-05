using System.Text.Json;
using Azure.Data.AppConfiguration;
using Azure.Identity;
using Dilcore.WebApi.Settings;

namespace Dilcore.WebApi.Extensions;

public static class ConfigurationExtensions
{
    extension(WebApplicationBuilder builder)
    {
        public void AddAppConfiguration()
    {
        var env = builder.Environment;

        // 1. Environment Variables
        builder.Configuration.AddEnvironmentVariables();

        // 2. User Secrets (if development env)
        if (env.IsDevelopment())
        {
            builder.Configuration.AddUserSecrets<Program>();
        }

        // 3. App Settings
        builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
        builder.Configuration.AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true);

        // 4. Azure App Config
        builder.LoadAzureAppConfiguration();
    }

    private void LoadAzureAppConfiguration()
    {
        var env = builder.Environment;
        var appConfigEndpoint = builder.Configuration[Constants.Configuration.AppConfigEndpointKey];

        if (string.IsNullOrEmpty(appConfigEndpoint))
        {
            return;
        }

        try
        {
            var credential = new DefaultAzureCredential();
            var client = new ConfigurationClient(new Uri(appConfigEndpoint), credential);

            // We fetch exactly two JSON configurations, both specifically labeled for the current environment.
            // Order is important: Shared first, then App-specific for precedence.
            var keysToFetch = new[] { Constants.Configuration.SharedKey, env.ApplicationName };
            var allConfigData = new Dictionary<string, string?>();

            foreach (var key in keysToFetch)
            {
                try
                {
                    var setting = client.GetConfigurationSetting(key, label: env.EnvironmentName);
                    if (setting?.Value != null && !string.IsNullOrEmpty(setting.Value.Value))
                    {
                        var kvps = ParseJson(setting.Value.Value);
                        foreach (var kvp in kvps)
                        {
                            allConfigData[kvp.Key] = kvp.Value;
                        }
                    }
                }
                catch (Azure.RequestFailedException ex) when (ex.Status == 404)
                {
                    // Configuration is optional, skip if not found
                    using var loggerFactory = LoggerFactory.Create(loggingBuilder => loggingBuilder.AddConsole());
                    var logger = loggerFactory.CreateLogger(typeof(ConfigurationExtensions));
                    logger.LogDebug(ex, "Optional configuration not found: {Key}", key); // Changed setting.Key to key as setting might be null
                }
            }

            if (allConfigData.Count > 0)
            {
                builder.Configuration.AddInMemoryCollection(allConfigData);
            }
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to load Azure App Configuration from endpoint '{appConfigEndpoint}'.", ex);
        }
    }
    }

    extension(IServiceCollection services)
    {
        public IServiceCollection AddAppSettings(IConfiguration configuration)
    {
        services.RegisterConfiguration<ApplicationSettings>(configuration);
        services.RegisterConfiguration<AuthenticationSettings>(configuration);
        return services;
    }

    public IServiceCollection RegisterConfiguration<T>(IConfiguration configuration) where T : class
    {
        var section = configuration.GetSection(typeof(T).Name);
        services.Configure<T>(section);
        return services;
    }
}

extension(IConfiguration configuration)
    {
        public T GetSettings<T>() where T : class, new()
{
    return configuration.GetSection(typeof(T).Name).Get<T>() ?? new T();
}

public T GetRequiredSettings<T>() where T : class
{
    var section = configuration.GetSection(typeof(T).Name);
    return section.Get<T>() ?? throw new InvalidOperationException($"Required configuration section '{typeof(T).Name}' is missing.");
}

public string GetValueOrDefault(string key, string defaultValue)
{
    return configuration[key] ?? defaultValue;
}
    }

    private static Dictionary<string, string?> ParseJson(string json)
{
    var data = new Dictionary<string, string?>();
    using var jsonDocument = JsonDocument.Parse(json);
    FlattenElement(jsonDocument.RootElement, data, string.Empty);
    return data;
}

private static void FlattenElement(JsonElement element, Dictionary<string, string?> data, string prefix)
{
    switch (element.ValueKind)
    {
        case JsonValueKind.Object:
            foreach (var property in element.EnumerateObject())
            {
                var key = string.IsNullOrEmpty(prefix) ? property.Name : $"{prefix}:{property.Name}";
                FlattenElement(property.Value, data, key);
            }
            break;
        case JsonValueKind.Array:
            int index = 0;
            foreach (var item in element.EnumerateArray())
            {
                var key = $"{prefix}:{index}";
                FlattenElement(item, data, key);
                index++;
            }
            break;
        default:
            data[prefix] = element.ToString();
            break;
    }
}
}
