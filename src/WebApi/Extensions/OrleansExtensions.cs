using Azure.Data.Tables;
using Dilcore.MultiTenant.Orleans.Extensions;
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
                    var serviceUri = new Uri(
                        $"https://{grainsSettings.StorageAccountName}.table.core.windows.net/");

                    options.TableServiceClient = new TableServiceClient(
                        serviceUri,
                        new Azure.Identity.DefaultAzureCredential());
                });
            }
            else
            {
                // Use localhost clustering when Azure clustering is disabled or misconfigured
                siloBuilder.UseLocalhostClustering();
            }

            siloBuilder.Configure<Orleans.Configuration.ClusterOptions>(options =>
            {
                options.ClusterId = grainsSettings.ClusterId;
                options.ServiceId = grainsSettings.ServiceId;
            });

            // Configure networking endpoints
            siloBuilder.ConfigureEndpoints(siloPort: 11111, gatewayPort: 30000);

            // In-memory grain storage
            siloBuilder.AddUserGrainStorage();
            siloBuilder.AddMemoryGrainStorage("TenantStore");

            // OpenTelemetry activity propagation
            siloBuilder.AddActivityPropagation();
            
            
            // Multi-tenancy support for Orleans
            siloBuilder.AddOrleansTenantContext();
        });
    }
}
