using Testcontainers.MongoDb;

namespace Dilcore.WebApi.IntegrationTests;

/// <summary>
/// Global fixture that manages all test containers for the integration tests.
/// Containers are started once before all tests and disposed after all tests complete.
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

    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        MongoDb = new MongoDbBuilder("mongo:8.0")
            .WithReplicaSet()
            .Build();

        await MongoDb.StartAsync();

        // Set environment variables for configuration binding
        // These are read by .NET configuration during Program.cs startup
        Environment.SetEnvironmentVariable("MongoDbSettings__ConnectionString", MongoDb.GetConnectionString());
        Environment.SetEnvironmentVariable("ConnectionStrings__MongoDb", MongoDb.GetConnectionString());
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
