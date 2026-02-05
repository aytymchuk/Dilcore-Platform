using Dilcore.Authentication.Abstractions;
using Dilcore.Tests.Common.Fixtures;
using Microsoft.Extensions.DependencyInjection;
using Moq;
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
            siloBuilder.UseInMemoryReminderService();
            siloBuilder.ConfigureServices(services =>
            {
                services.AddSingleton(TimeProvider.System);

                var userContext = new UserContext("test-user-id", "test@example.com", "Test User", [], []);
                services.AddSingleton<IUserContext>(userContext);

                var mockResolver = new Mock<IUserContextResolver>();
                IUserContext? outContext = userContext;
                mockResolver.Setup(r => r.TryResolve(out outContext)).Returns(true);
                mockResolver.Setup(r => r.Resolve()).Returns(userContext);
                services.AddSingleton<IUserContextResolver>(mockResolver.Object);
            });
        }
    }
}
