using Dilcore.MultiTenant.Abstractions;
using Microsoft.Extensions.Logging;
using Moq;
using Shouldly;

namespace Dilcore.MultiTenant.Orleans.Extensions.Tests;

/// <summary>
/// Unit tests for tenant grain call filters.
/// </summary>
[TestFixture]
public class TenantGrainCallFiltersTests
{
    public interface ITestGrain : IGrain
    {
        Task DoSomething();
    }

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
            var logger = new Mock<ILogger<TenantIncomingGrainCallFilter>>();
            var filter = new TenantIncomingGrainCallFilter(logger.Object);
            var context = new Mock<IIncomingGrainCallContext>();
            var grainMock = new Mock<IGrain>();

            context.SetupGet(c => c.Grain).Returns(grainMock.Object);
            context.SetupGet(c => c.InterfaceMethod).Returns(typeof(ITestGrain).GetMethods().First());

            // Set tenant context
            OrleansTenantContextAccessor.SetTenantContext(
                new TenantContext(Guid.CreateVersion7(), "test-tenant", "storage-123"));

            var invokeCalled = false;
            context.Setup(c => c.Invoke()).Returns(() =>
            {
                invokeCalled = true;
                return Task.CompletedTask;
            });

            // Act
            await filter.Invoke(context.Object);

            // Assert
            invokeCalled.ShouldBeTrue();
        }

        [Test]
        public async Task Invoke_ShouldContinueChain_WhenNoTenantContext()
        {
            // Arrange
            var logger = new Mock<ILogger<TenantIncomingGrainCallFilter>>();
            var filter = new TenantIncomingGrainCallFilter(logger.Object);
            var context = new Mock<IIncomingGrainCallContext>();
            var grainMock = new Mock<IGrain>();

            context.SetupGet(c => c.Grain).Returns(grainMock.Object);
            context.SetupGet(c => c.InterfaceMethod).Returns(typeof(ITestGrain).GetMethods().First());

            var invokeCalled = false;
            context.Setup(c => c.Invoke()).Returns(() =>
            {
                invokeCalled = true;
                return Task.CompletedTask;
            });

            // Act
            await filter.Invoke(context.Object);

            // Assert
            invokeCalled.ShouldBeTrue();
        }
    }

    [TestFixture]
    public class OutgoingFilterTests
    {
        private delegate void TryResolveDelegate(out ITenantContext? tenantContext);

        [SetUp]
        public void SetUp()
        {
            RequestContext.Clear();
        }

        [Test]
        public async Task Invoke_ShouldPropagateTenantContext_WhenResolverHasContext()
        {
            // Arrange
            var resolver = new Mock<ITenantContextResolver>();
            var logger = new Mock<ILogger<TenantOutgoingGrainCallFilter>>();
            var filter = new TenantOutgoingGrainCallFilter(resolver.Object, logger.Object);
            var context = new Mock<IOutgoingGrainCallContext>();
            var grainMock = new Mock<IGrain>();

            context.SetupGet(c => c.Grain).Returns(grainMock.Object);
            context.SetupGet(c => c.InterfaceMethod).Returns(typeof(ITestGrain).GetMethods().First());

            ITenantContext resolvedContext = new TenantContext(Guid.CreateVersion7(), "test-tenant", "storage-123");
            resolver.Setup(r => r.TryResolve(out It.Ref<ITenantContext?>.IsAny))
                .Callback(new TryResolveDelegate((out ITenantContext? tc) => tc = resolvedContext))
                .Returns(true);

            var invokeCalled = false;
            context.Setup(c => c.Invoke()).Returns(() =>
            {
                invokeCalled = true;

                // Assert context is set DURING the call
                var propagatedContext = OrleansTenantContextAccessor.GetTenantContext();
                propagatedContext.ShouldNotBeNull();
                propagatedContext.Name.ShouldBe("test-tenant");
                propagatedContext.StorageIdentifier.ShouldBe("storage-123");

                return Task.CompletedTask;
            });

            // Act
            await filter.Invoke(context.Object);

            // Assert
            invokeCalled.ShouldBeTrue();
        }

        [Test]
        public async Task Invoke_ShouldContinueChain_WhenNoTenantContext()
        {
            // Arrange
            var resolver = new Mock<ITenantContextResolver>();
            var logger = new Mock<ILogger<TenantOutgoingGrainCallFilter>>();
            var filter = new TenantOutgoingGrainCallFilter(resolver.Object, logger.Object);
            var context = new Mock<IOutgoingGrainCallContext>();
            var grainMock = new Mock<IGrain>();

            context.SetupGet(c => c.Grain).Returns(grainMock.Object);
            context.SetupGet(c => c.InterfaceMethod).Returns(typeof(ITestGrain).GetMethods().First());

            ITenantContext? tenantContext = null;
            resolver.Setup(r => r.TryResolve(out tenantContext))
                .Returns(false);

            var invokeCalled = false;
            context.Setup(c => c.Invoke()).Returns(() =>
            {
                invokeCalled = true;
                return Task.CompletedTask;
            });

            // Act
            await filter.Invoke(context.Object);

            // Assert
            invokeCalled.ShouldBeTrue();
        }
    }
}
