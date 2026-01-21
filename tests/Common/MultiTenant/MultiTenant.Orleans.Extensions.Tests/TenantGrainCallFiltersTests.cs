using Dilcore.MultiTenant.Abstractions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Orleans;
using Orleans.Runtime;
using Shouldly;

namespace Dilcore.MultiTenant.Orleans.Extensions.Tests;

/// <summary>
/// Unit tests for tenant grain call filters.
/// </summary>
[TestFixture]
public class TenantGrainCallFiltersTests
{
    [SetUp]
    public void SetUp()
    {
        RequestContext.Clear();
    }

    [TestFixture]
    public class IncomingFilterTests
    {
        [SetUp]
        public void SetUp()
        {
            RequestContext.Clear();
        }

        [Test]
        public async Task Invoke_ShouldLogTenantContext_WhenContextExists()
        {
            // Arrange
            var logger = Substitute.For<ILogger<TenantIncomingGrainCallFilter>>();
            var filter = new TenantIncomingGrainCallFilter(logger);
            var context = Substitute.For<IIncomingGrainCallContext>();
            var grainMock = Substitute.For<IGrain>();

            context.Grain.Returns(grainMock);
            context.InterfaceMethod.Returns(typeof(IGrain).GetMethods().First());

            // Set tenant context
            OrleansTenantContextAccessor.SetTenantContext(
                new TenantContext("test-tenant", "storage-123"));

            var invokeCalled = false;
            context.Invoke().Returns(_ =>
            {
                invokeCalled = true;
                return Task.CompletedTask;
            });

            // Act
            await filter.Invoke(context);

            // Assert
            invokeCalled.ShouldBeTrue();
            logger.ReceivedCalls().ShouldNotBeEmpty();
        }

        [Test]
        public async Task Invoke_ShouldContinueChain_WhenNoTenantContext()
        {
            // Arrange
            var logger = Substitute.For<ILogger<TenantIncomingGrainCallFilter>>();
            var filter = new TenantIncomingGrainCallFilter(logger);
            var context = Substitute.For<IIncomingGrainCallContext>();
            var grainMock = Substitute.For<IGrain>();

            context.Grain.Returns(grainMock);
            context.InterfaceMethod.Returns(typeof(IGrain).GetMethods().First());

            var invokeCalled = false;
            context.Invoke().Returns(_ =>
            {
                invokeCalled = true;
                return Task.CompletedTask;
            });

            // Act
            await filter.Invoke(context);

            // Assert
            invokeCalled.ShouldBeTrue();
        }
    }

    [TestFixture]
    public class OutgoingFilterTests
    {
        [SetUp]
        public void SetUp()
        {
            RequestContext.Clear();
        }

        [Test]
        public async Task Invoke_ShouldPropagateTenantContext_WhenResolverHasContext()
        {
            // Arrange
            var resolver = Substitute.For<ITenantContextResolver>();
            var logger = Substitute.For<ILogger<TenantOutgoingGrainCallFilter>>();
            var filter = new TenantOutgoingGrainCallFilter(resolver, logger);
            var context = Substitute.For<IOutgoingGrainCallContext>();
            var grainMock = Substitute.For<IGrain>();

            context.Grain.Returns(grainMock);
            context.InterfaceMethod.Returns(typeof(IGrain).GetMethods().First());

            var tenantContext = new TenantContext("test-tenant", "storage-123");
            resolver.TryResolve(out Arg.Any<ITenantContext?>())
                .Returns(x =>
                {
                    x[0] = tenantContext;
                    return true;
                });

            var invokeCalled = false;
            context.Invoke().Returns(_ =>
            {
                invokeCalled = true;
                return Task.CompletedTask;
            });

            // Act
            await filter.Invoke(context);

            // Assert
            invokeCalled.ShouldBeTrue();
            var propagatedContext = OrleansTenantContextAccessor.GetTenantContext();
            propagatedContext.ShouldNotBeNull();
            propagatedContext.Name.ShouldBe("test-tenant");
            propagatedContext.StorageIdentifier.ShouldBe("storage-123");
        }

        [Test]
        public async Task Invoke_ShouldContinueChain_WhenNoTenantContext()
        {
            // Arrange
            var resolver = Substitute.For<ITenantContextResolver>();
            var logger = Substitute.For<ILogger<TenantOutgoingGrainCallFilter>>();
            var filter = new TenantOutgoingGrainCallFilter(resolver, logger);
            var context = Substitute.For<IOutgoingGrainCallContext>();
            var grainMock = Substitute.For<IGrain>();

            context.Grain.Returns(grainMock);
            context.InterfaceMethod.Returns(typeof(IGrain).GetMethods().First());

            resolver.TryResolve(out Arg.Any<ITenantContext?>())
                .Returns(x =>
                {
                    x[0] = null;
                    return false;
                });

            var invokeCalled = false;
            context.Invoke().Returns(_ =>
            {
                invokeCalled = true;
                return Task.CompletedTask;
            });

            // Act
            await filter.Invoke(context);

            // Assert
            invokeCalled.ShouldBeTrue();
        }
    }
}
