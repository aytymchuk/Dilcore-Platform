using System.Text.Json;
using Azure.Data.AppConfiguration;
using Azure.Identity;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Dilcore.Configuration.AspNetCore;

public static class ConfigurationBuilderExtensions
{
    private static readonly ILogger _logger = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger(typeof(ConfigurationBuilderExtensions));

    public static void AddAppConfiguration(this WebApplicationBuilder builder)
    {
        var env = builder.Environment;

        // 1. Environment Variables
        builder.Configuration.AddEnvironmentVariables();

        // 2. User Secrets (if development env)
        if (env.IsDevelopment())
        {
            // We cannot generic AddUserSecrets<Program> here easily because we don't reference Program.
            // But usually AddUserSecrets is assembly based.
            // However, builder.Configuration.AddUserSecrets(Assembly.GetEntryAssembly()) works?
            // Or just rely on default builder behavior?
            // WebApplication.CreateBuilder(args) ALREADY adds user secrets in Dev.
            // But ConfigurationExtensions in WebApi added it explicitly: builder.Configuration.AddUserSecrets<Program>();

            // If we want to support it, we might need to pass the assembly or T.
            // For now, let's skip explicit UserSecrets adding if generic is generic, OR assume entry assembly.
            try
            {
                var entryAssembly = System.Reflection.Assembly.GetEntryAssembly();
                if (entryAssembly != null)
                {
                    builder.Configuration.AddUserSecrets(entryAssembly, optional: true);
                }
            }
            catch
            {
                // ignore
            }
        }

        // 3. App Settings
        builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
        builder.Configuration.AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true);

        // 4. Azure App Config
        builder.LoadAzureAppConfiguration();
    }

    private static void LoadAzureAppConfiguration(this WebApplicationBuilder builder)
    {
        var env = builder.Environment;
        var appConfigEndpoint = builder.Configuration[Constants.AppConfigEndpointKey];

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
            var keysToFetch = new[] { Constants.SharedKey, env.ApplicationName };
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
                    _logger.LogOptionalConfigurationNotFound(ex, key);
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