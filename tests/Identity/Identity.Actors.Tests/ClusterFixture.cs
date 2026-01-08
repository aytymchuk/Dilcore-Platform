using Microsoft.Extensions.Hosting;
using Orleans.TestingHost;

namespace Dilcore.Identity.Actors.Tests;

/// <summary>
/// Configures the TestCluster for grain tests.
/// </summary>
public class ClusterFixture : IDisposable
{
    public TestCluster Cluster { get; }

    public ClusterFixture()
    {
        var builder = new TestClusterBuilder();
        builder.AddSiloBuilderConfigurator<TestSiloConfigurator>();
        Cluster = builder.Build();
        Cluster.Deploy();
    }

    public void Dispose()
    {
        Cluster.StopAllSilos();
        GC.SuppressFinalize(this);
    }

    private sealed class TestSiloConfigurator : ISiloConfigurator
    {
        public void Configure(ISiloBuilder siloBuilder)
        {
            // Use in-memory storage for testing
            siloBuilder.AddMemoryGrainStorage("UserStore");
        }
    }
}
