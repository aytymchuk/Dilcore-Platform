using Testcontainers.MongoDb;

namespace Dilcore.WebApi.IntegrationTests;

[SetUpFixture]
public class SharedMongoFixture
{
    public static MongoDbContainer Container { get; private set; } = null!;

    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        Container = new MongoDbBuilder("mongo:8.0")
            .WithReplicaSet()
            .Build();

        await Container.StartAsync();

        Environment.SetEnvironmentVariable("MongoDbSettings__ConnectionString", Container.GetConnectionString());
        Environment.SetEnvironmentVariable("ConnectionStrings__MongoDb", Container.GetConnectionString());
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDown()
    {
        await Container.DisposeAsync();

        Environment.SetEnvironmentVariable("MongoDbSettings__ConnectionString", null);
        Environment.SetEnvironmentVariable("ConnectionStrings__MongoDb", null);
    }
}
