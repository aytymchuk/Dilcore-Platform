using Dilcore.Tenancy.Actors.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Orleans;

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

    protected static async Task SeedTenantAsync(CustomWebApplicationFactory factory, string tenantId)
    {
        using var scope = factory.Services.CreateScope();
        var grainFactory = scope.ServiceProvider.GetRequiredService<IGrainFactory>();
        var tenantGrain = grainFactory.GetGrain<ITenantGrain>(tenantId);

        var existing = await tenantGrain.GetAsync();
        if (existing is null)
        {
            await tenantGrain.CreateAsync($"Test Tenant {tenantId}", "Seeded for Integration Tests");
        }
    }
}