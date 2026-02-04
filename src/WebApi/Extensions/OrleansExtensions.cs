using Azure.Data.Tables;
using Dilcore.Identity.Actors;
using Dilcore.Authentication.Orleans.Extensions;
using Dilcore.MultiTenant.Orleans.Extensions;
using Dilcore.Tenancy.Actors;
using Dilcore.WebApi.Settings;

namespace Dilcore.WebApi.Extensions;

internal static class OrleansExtensions
{
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
                // Azure Storage clustering with Managed Identity
                siloBuilder.UseAzureStorageClustering(options =>
                {
                    options.TableServiceClient = CreateTableServiceClient(grainsSettings.StorageAccountName);
                });

                // Azure Table Reminders
                siloBuilder.UseAzureTableReminderService(options =>
                {
                    options.TableServiceClient = CreateTableServiceClient(grainsSettings.StorageAccountName);
                });
            }
            else
            {
                // Use localhost clustering when Azure clustering is disabled or misconfigured
                siloBuilder.UseLocalhostClustering();

                // Use in-memory reminders for local development
                siloBuilder.UseInMemoryReminderService();
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

    private static TableServiceClient CreateTableServiceClient(string storageAccountName)
    {
        var serviceUri = new Uri($"https://{storageAccountName}.table.core.windows.net/");
        return new TableServiceClient(serviceUri, new Azure.Identity.DefaultAzureCredential());
    }
}
