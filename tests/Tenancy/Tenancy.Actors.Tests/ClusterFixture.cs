using Dilcore.Tests.Common.Fixtures;
using Orleans.TestingHost;

namespace Dilcore.Tenancy.Actors.Tests;

/// <summary>
/// Configures the TestCluster for grain tests.
/// </summary>
public class ClusterFixture : CommonClusterFixture<ClusterFixture.TenantStoreConfigurator>
{
    public class TenantStoreConfigurator : ISiloConfigurator
    {
        public void Configure(ISiloBuilder siloBuilder)
        {
            siloBuilder.AddMemoryGrainStorage("TenantStore");
        }
    }
}
