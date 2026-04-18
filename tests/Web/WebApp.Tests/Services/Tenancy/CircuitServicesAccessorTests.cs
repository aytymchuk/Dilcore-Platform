using Dilcore.WebApp.Services.Tenancy;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace Dilcore.WebApp.Tests.Services.Tenancy;

[TestFixture]
public class CircuitServicesAccessorTests
{
    [TearDown]
    public void TearDown()
    {
        new CircuitServicesAccessor().Services = null;
    }

    [Test]
    public void Services_IsNull_ByDefault()
    {
        var accessor = new CircuitServicesAccessor();

        accessor.Services.ShouldBeNull();
    }

    [Test]
    public void Services_RoundTrips_SetValue()
    {
        var accessor = new CircuitServicesAccessor();
        using var provider = new ServiceCollection().BuildServiceProvider();

        accessor.Services = provider;

        accessor.Services.ShouldBe(provider);
    }

    [Test]
    public async Task Services_Persists_AcrossAwait_OnSameAsyncFlow()
    {
        var accessor = new CircuitServicesAccessor();
        await using var provider = new ServiceCollection().BuildServiceProvider();
        accessor.Services = provider;

        await Task.Yield();

        accessor.Services.ShouldBe(provider);
    }

    [Test]
    public void Instances_OnSameExecutionContext_ShareServicesSlot()
    {
        using var provider = new ServiceCollection().BuildServiceProvider();
        var writer = new CircuitServicesAccessor();
        var reader = new CircuitServicesAccessor();

        writer.Services = provider;

        reader.Services.ShouldBe(provider);
    }
}
