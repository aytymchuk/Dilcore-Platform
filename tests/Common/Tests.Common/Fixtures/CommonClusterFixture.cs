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
        try
        {
            Cluster.StopAllSilos();
        }
        catch (Exception)
        {
            // Ignore exceptions during shutdown to ensure Dispose doesn't throw
        }
        finally
        {
            GC.SuppressFinalize(this);
        }
    }
}