namespace Dilcore.WebApi.IntegrationTests.Infrastructure;

[TestFixture]
public abstract class BaseIntegrationTest
{
    protected CustomWebApplicationFactory Factory { get; private set; } = null!;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        Factory = new CustomWebApplicationFactory();
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDown()
    {
        await Factory.DisposeAsync();
    }
}