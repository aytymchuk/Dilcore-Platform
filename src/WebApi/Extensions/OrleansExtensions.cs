using Azure.Data.Tables;
using Dilcore.Identity.Actors;
using Dilcore.Authentication.Orleans.Extensions;
using Dilcore.MultiTenant.Orleans.Extensions;
using Dilcore.Tenancy.Actors;
using Dilcore.WebApi.Settings;
using Azure.Identity;
using System.Collections.Concurrent;

namespace Dilcore.WebApi.Extensions;

internal static class OrleansExtensions
{
    private static readonly ConcurrentDictionary<string, TableServiceClient> _tableServiceClients = new();

    public static IHostBuilder AddOrleansConfiguration(this IHostBuilder hostBuilder)
    {
        return hostBuilder.UseOrleans((context, siloBuilder) =>
        {
            var grainsSettings = context.Configuration
                .GetSection(nameof(GrainsSettings))
                .Get<GrainsSettings>() ?? new GrainsSettings();

            // Skip Orleans Azure clustering if StorageAccountName is missing
            if (!string.IsNullOrWhiteSpace(grainsSettings.StorageAccountName))
            {
                siloBuilder.UseAzureTableStorage(grainsSettings.StorageAccountName);
            }
            else
            {
                siloBuilder.UseLocalhostServices();
            }

            siloBuilder.Configure<Orleans.Configuration.ClusterOptions>(options =>
            {
                options.ClusterId = grainsSettings.ClusterId;
                options.ServiceId = grainsSettings.ServiceId;
            });

            // Configure networking endpoints
            siloBuilder.ConfigureEndpoints(
                siloPort: grainsSettings.SiloPort,
                gatewayPort: grainsSettings.GatewayPort);

            // Identity custom grain storage
            siloBuilder.AddUserGrainStorage();
            // Register Tenant Grain Storage
            siloBuilder.AddTenancyActors();

            // OpenTelemetry activity propagation
            siloBuilder.AddActivityPropagation();

            // Multi-tenancy support for Orleans
            siloBuilder.AddOrleansTenantContext();

            // User context support for Orleans
            siloBuilder.AddOrleansUserContext();

            siloBuilder.AddReminders();
        });
    }

    private static void UseLocalhostServices(this ISiloBuilder siloBuilder)
    {
        // Use localhost clustering when Azure clustering is disabled or misconfigured
        siloBuilder.UseLocalhostClustering();

        // Use in-memory reminders for local development
        siloBuilder.UseInMemoryReminderService();
    }

    private static void UseAzureTableStorage(this ISiloBuilder siloBuilder, string storageAccountName)
    {
        var client = GetTableServiceClient(storageAccountName);

        // Azure Storage clustering with Managed Identity
        siloBuilder.UseAzureStorageClustering(options =>
        {
            options.TableServiceClient = client;
        });

        // Azure Table Reminders
        siloBuilder.UseAzureTableReminderService(options =>
        {
            options.TableServiceClient = client;
        });
    }

    private static TableServiceClient GetTableServiceClient(string storageAccountName)
    {
        var serviceUri = new Uri($"https://{storageAccountName}.table.core.windows.net/");
        return new TableServiceClient(serviceUri, new DefaultAzureCredential());
    }
}
