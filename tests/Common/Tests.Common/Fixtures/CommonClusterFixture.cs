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

    public virtual void Dispose()
    {
        try
        {
            Cluster.StopAllSilos();
        }
        catch (Exception ex)
        {
            // Log exception to ensure visibility, but don't rethrow
            Console.WriteLine($"Error during cluster shutdown: {ex}");
        }

        GC.SuppressFinalize(this);
    }
}