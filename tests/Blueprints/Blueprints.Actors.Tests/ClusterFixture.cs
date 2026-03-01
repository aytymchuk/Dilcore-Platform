using Dilcore.Tests.Common.Fixtures;
using Microsoft.Extensions.DependencyInjection;
using Orleans.TestingHost;

namespace Dilcore.Blueprints.Actors.Tests;

public class ClusterFixture : CommonClusterFixture<ClusterFixture.BlueprintStoreConfigurator>
{
    public class BlueprintStoreConfigurator : ISiloConfigurator
    {
        public void Configure(ISiloBuilder siloBuilder)
        {
            siloBuilder.AddMemoryGrainStorage(SiloBuilderExtensions.StoreName);
            siloBuilder.ConfigureServices(services =>
            {
                services.AddSingleton(TimeProvider.System);
            });
        }
    }
}
