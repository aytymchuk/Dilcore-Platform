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
    /// <exception cref="InvalidOperationException">Thrown when Docker is not available or the container was not started.</exception>
    public static string MongoDbConnectionString =>
        IsDockerAvailable && MongoDb is not null
            ? MongoDb.GetConnectionString()
            : throw new InvalidOperationException("MongoDB connection string is not available. Docker may not be running or container setup failed.");

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
        if (MongoDb is not null)
        {
            try
            {
                await MongoDb.DisposeAsync();
            }
            catch
            {
                // Swallow disposal errors when Docker was unavailable during setup
            }
        }

        // Clear environment variables
        Environment.SetEnvironmentVariable("MongoDbSettings__ConnectionString", null);
        Environment.SetEnvironmentVariable("ConnectionStrings__MongoDb", null);
    }
}
