using DotNet.Testcontainers.Builders;
using Testcontainers.MongoDb;

namespace Dilcore.WebApi.IntegrationTests;

/// <summary>
/// Global fixture that manages all test containers for the integration tests.
/// Containers are started once before all tests and disposed after all tests complete.
/// Skips all integration tests when Docker is not available.
/// </summary>
[SetUpFixture]
public class TestContainerFixture
{
    /// <summary>
    /// MongoDB container instance. Connection string available via <see cref="MongoDbConnectionString"/>.
    /// </summary>
    public static MongoDbContainer MongoDb { get; private set; } = null!;

    /// <summary>
    /// MongoDB connection string from the running container.
    /// </summary>
    public static string MongoDbConnectionString => MongoDb.GetConnectionString();

    /// <summary>
    /// True when Docker was available and the MongoDB container started successfully.
    /// </summary>
    public static bool IsDockerAvailable { get; private set; }

    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        try
        {
            MongoDb = new MongoDbBuilder("mongo:8.0")
                .WithReplicaSet()
                .Build();

            await MongoDb.StartAsync();
            IsDockerAvailable = true;

            // Set environment variables for configuration binding
            // These are read by .NET configuration during Program.cs startup
            Environment.SetEnvironmentVariable("MongoDbSettings__ConnectionString", MongoDb.GetConnectionString());
            Environment.SetEnvironmentVariable("ConnectionStrings__MongoDb", MongoDb.GetConnectionString());
        }
        catch (DockerUnavailableException)
        {
            Assert.Ignore("Docker is not available. Start Docker to run integration tests.");
        }
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDown()
    {
        await MongoDb.DisposeAsync();

        // Clear environment variables
        Environment.SetEnvironmentVariable("MongoDbSettings__ConnectionString", null);
        Environment.SetEnvironmentVariable("ConnectionStrings__MongoDb", null);
    }
}
