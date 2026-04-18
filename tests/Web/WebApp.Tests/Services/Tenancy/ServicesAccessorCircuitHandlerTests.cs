using System.Runtime.Serialization;
using Dilcore.WebApp.Services.Tenancy;
using Microsoft.AspNetCore.Components.Server.Circuits;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace Dilcore.WebApp.Tests.Services.Tenancy;

[TestFixture]
public class ServicesAccessorCircuitHandlerTests
{
    [TearDown]
    public void TearDown()
    {
        new CircuitServicesAccessor().Services = null;
    }

    [Test]
    public async Task CreateInboundActivityHandler_SetsCircuitServicesDuringNext_ThenClears()
    {
        await using var services = new ServiceCollection().BuildServiceProvider();
        var accessor = new CircuitServicesAccessor();
        var sut = new ServicesAccessorCircuitHandler(services, accessor);

        IServiceProvider? seenDuringNext = null;
        var pipeline = sut.CreateInboundActivityHandler(_ =>
        {
            seenDuringNext = accessor.Services;
            return Task.CompletedTask;
        });

        accessor.Services.ShouldBeNull();

        await pipeline(UninitializedInboundContext());

        accessor.Services.ShouldBeNull();
        seenDuringNext.ShouldBe(services);
    }

    [Test]
    public async Task CreateInboundActivityHandler_ClearsCircuitServices_WhenNextThrows()
    {
        await using var services = new ServiceCollection().BuildServiceProvider();
        var accessor = new CircuitServicesAccessor();
        var sut = new ServicesAccessorCircuitHandler(services, accessor);

        var pipeline = sut.CreateInboundActivityHandler(_ => Task.FromException(new InvalidOperationException("boom")));

        await Should.ThrowAsync<InvalidOperationException>(() => pipeline(UninitializedInboundContext()));

        accessor.Services.ShouldBeNull();
    }

    private static CircuitInboundActivityContext UninitializedInboundContext()
    {
        // Framework type has no public ctor; only the handler passes it through to "next".
#pragma warning disable SYSLIB0050
        return (CircuitInboundActivityContext)FormatterServices.GetUninitializedObject(typeof(CircuitInboundActivityContext));
#pragma warning restore SYSLIB0050
    }
}
