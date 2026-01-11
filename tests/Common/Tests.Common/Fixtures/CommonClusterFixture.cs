using Orleans.TestingHost;

namespace Dilcore.Tests.Common.Fixtures;

public class CommonClusterFixture<TConfigurator> : IDisposable
    where TConfigurator : class, ISiloConfigurator, new()
{
    public TestCluster Cluster { get; }

    public CommonClusterFixture()
    {
        var builder = new TestClusterBuilder();
        builder.AddSiloBuilderConfigurator<TConfigurator>();
        Cluster = builder.Build();
        Cluster.Deploy();
    }

    public void Dispose()
    {
        Cluster.StopAllSilos();
        GC.SuppressFinalize(this);
    }
}