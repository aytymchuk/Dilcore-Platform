using Dilcore.MultiTenant.Abstractions;
using Dilcore.Tests.Common.Fixtures;
using Microsoft.Extensions.DependencyInjection;
using Orleans.TestingHost;

namespace Dilcore.MultiTenant.Orleans.Extensions.Tests;

/// <summary>
/// Configures the TestCluster for tenant context integration tests.
/// </summary>
public class ClusterFixture : CommonClusterFixture<ClusterFixture.TenantContextConfigurator>
{
    public class TenantContextConfigurator : ISiloConfigurator
    {
        public void Configure(ISiloBuilder siloBuilder)
        {
            // Add Orleans tenant context integration
            siloBuilder.AddOrleansTenantContext();

            // Register tenant context resolver
            siloBuilder.ConfigureServices(services =>
            {
                services.AddSingleton<ITenantContextResolver, TenantContextResolver>();
            });
        }
    }
}
