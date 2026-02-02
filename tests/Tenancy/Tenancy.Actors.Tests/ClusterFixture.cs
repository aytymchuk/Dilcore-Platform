using Dilcore.Authentication.Abstractions;
using Dilcore.Tests.Common.Fixtures;
using Microsoft.Extensions.DependencyInjection;
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
            siloBuilder.AddMemoryGrainStorage("UserStore");
            siloBuilder.ConfigureServices(services =>
            {
                services.AddSingleton(TimeProvider.System);
                services.AddSingleton<IUserContext>(new UserContext("test-user-id", "test@example.com", "Test User"));
            });
        }
    }
}
