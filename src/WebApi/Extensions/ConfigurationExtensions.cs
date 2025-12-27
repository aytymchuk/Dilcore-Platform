using System.Text.Json;
using Azure.Data.AppConfiguration;
using Azure.Identity;

namespace Dilcore.WebApi.Extensions;

public static class ConfigurationExtensions
{
    private static class AppConfigConstants
    {
        public const string SharedKey = "Shared";
        public const string AppConfigEndpointKey = "AppConfigEndpoint";
    }

    public static void AddAppConfiguration(this WebApplicationBuilder builder)
    {
        // Clear default sources to enforce specific order
        builder.Configuration.Sources.Clear();

        var env = builder.Environment;

        // 1. Environment Variables
        builder.Configuration.AddEnvironmentVariables();

        // 2. User Secrets (if development env)
        if (env.IsDevelopment())
        {
            builder.Configuration.AddUserSecrets<Program>();
        }

        // 3. App Settings
        builder.Configuration.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
        builder.Configuration.AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true);

        // 4. Azure App Config
        builder.LoadAzureAppConfiguration();
    }

    private static void LoadAzureAppConfiguration(this WebApplicationBuilder builder)
    {
        var env = builder.Environment;
        var appConfigEndpoint = builder.Configuration[AppConfigConstants.AppConfigEndpointKey];

        if (string.IsNullOrEmpty(appConfigEndpoint)) return;

        try
        {
            var credential = new DefaultAzureCredential();
            var client = new ConfigurationClient(new Uri(appConfigEndpoint), credential);

            // We fetch exactly two JSON configurations, both specifically labeled for the current environment.
            // Order is important: Shared first, then App-specific for precedence.
            var keysToFetch = new[] { AppConfigConstants.SharedKey, env.ApplicationName };
            var allConfigData = new List<KeyValuePair<string, string?>>();

            foreach (var key in keysToFetch)
            {
                try
                {
                    var setting = client.GetConfigurationSetting(key, label: env.EnvironmentName);
                    if (setting?.Value != null && !string.IsNullOrEmpty(setting.Value.Value))
                    {
                        var kvps = ParseJson(setting.Value.Value);
                        allConfigData.AddRange(kvps);
                    }
                }
                catch (Azure.RequestFailedException ex) when (ex.Status == 404)
                {
                    // Configuration is optional, skip if not found
                }
            }

            if (allConfigData.Any())
            {
                builder.Configuration.AddInMemoryCollection(allConfigData);
            }
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to load Azure App Configuration from endpoint '{appConfigEndpoint}'.", ex);
        }
    }

    public static IServiceCollection RegisterConfiguration<T>(this IServiceCollection services, IConfiguration configuration, string? sectionName = null) where T : class
    {
        sectionName ??= typeof(T).Name;
        var section = configuration.GetSection(sectionName);
        services.Configure<T>(section);
        return services;
    }

    private static Dictionary<string, string?> ParseJson(string json)
    {
        var data = new Dictionary<string, string?>();
        var jsonDocument = JsonDocument.Parse(json);
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
