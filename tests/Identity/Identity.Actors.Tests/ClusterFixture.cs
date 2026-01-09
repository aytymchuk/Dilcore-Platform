using Dilcore.Tests.Common.Fixtures;
using Orleans.TestingHost;

namespace Dilcore.Identity.Actors.Tests;

/// <summary>
/// Configures the TestCluster for grain tests.
/// </summary>
public class ClusterFixture : CommonClusterFixture<ClusterFixture.UserStoreConfigurator>
{
    public class UserStoreConfigurator : ISiloConfigurator
    {
        public void Configure(ISiloBuilder siloBuilder)
        {
            siloBuilder.AddMemoryGrainStorage("UserStore");
        }
    }
}
