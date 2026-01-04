using Dilcore.MultiTenant.Abstractions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Shouldly;

namespace Dilcore.MultiTenant.Http.Extensions.Tests;

[TestFixture]
public class TenantContextResolverTests
{
    [Test]
    public void Resolve_WithSuccessfulProvider_ReturnsContext()
    {
        // Arrange
        var expectedContext = new TenantContext("T1", "storage-01");

        var providerMock = new Mock<ITenantContextProvider>();
        providerMock.Setup(x => x.Priority).Returns(100);
        providerMock.Setup(x => x.GetTenantContext()).Returns(expectedContext);

        var resolver = new TenantContextResolver(new[] { providerMock.Object }, NullLogger<TenantContextResolver>.Instance);

        // Act
        var result = resolver.Resolve();

        // Assert
        result.ShouldBe(expectedContext);
    }

    [Test]
    public void Resolve_CalledMultipleTimes_ResolvesOnlyOnce()
    {
        // Arrange
        var context = new TenantContext("T1", "storage-01");

        var providerMock = new Mock<ITenantContextProvider>();
        providerMock.Setup(x => x.Priority).Returns(100);
        providerMock.Setup(x => x.GetTenantContext()).Returns(context);

        var resolver = new TenantContextResolver(new[] { providerMock.Object }, NullLogger<TenantContextResolver>.Instance);

        // Act
        var result1 = resolver.Resolve();
        var result2 = resolver.Resolve();

        // Assert
        result1.ShouldBe(result2);
        providerMock.Verify(x => x.GetTenantContext(), Times.Exactly(2)); // Stateless - called each time
    }

    [Test]
    public void Resolve_MultipleProviders_UsesHighestPriority()
    {
        // Arrange
        var lowContext = new TenantContext("Low", "low-storage");
        var lowProviderMock = new Mock<ITenantContextProvider>();
        lowProviderMock.Setup(x => x.Priority).Returns(50);
        lowProviderMock.Setup(x => x.GetTenantContext()).Returns(lowContext);

        var highContext = new TenantContext("High", "high-storage");
        var highProviderMock = new Mock<ITenantContextProvider>();
        highProviderMock.Setup(x => x.Priority).Returns(200);
        highProviderMock.Setup(x => x.GetTenantContext()).Returns(highContext);

        var resolver = new TenantContextResolver(
            new[] { lowProviderMock.Object, highProviderMock.Object },
            NullLogger<TenantContextResolver>.Instance);

        // Act
        var result = resolver.Resolve();

        // Assert
        result.ShouldBe(highContext);
        highProviderMock.Verify(x => x.GetTenantContext(), Times.Once);
        lowProviderMock.Verify(x => x.GetTenantContext(), Times.Never); // Not called - higher priority succeeded
    }

    [Test]
    public void Resolve_HighPriorityReturnsNull_FallsBackToLowerPriority()
    {
        // Arrange
        var lowContext = new TenantContext("Low", "low-storage");
        var lowProviderMock = new Mock<ITenantContextProvider>();
        lowProviderMock.Setup(x => x.Priority).Returns(50);
        lowProviderMock.Setup(x => x.GetTenantContext()).Returns(lowContext);

        var highProviderMock = new Mock<ITenantContextProvider>();
        highProviderMock.Setup(x => x.Priority).Returns(200);
        highProviderMock.Setup(x => x.GetTenantContext()).Returns((ITenantContext?)null);

        var resolver = new TenantContextResolver(
            new[] { lowProviderMock.Object, highProviderMock.Object },
            NullLogger<TenantContextResolver>.Instance);

        // Act
        var result = resolver.Resolve();

        // Assert
        result.ShouldBe(lowContext);
        highProviderMock.Verify(x => x.GetTenantContext(), Times.Once);
        lowProviderMock.Verify(x => x.GetTenantContext(), Times.Once); // Called after high priority failed
    }

    [Test]
    public void Resolve_AllProvidersFail_ReturnsEmpty()
    {
        // Arrange
        var provider1Mock = new Mock<ITenantContextProvider>();
        provider1Mock.Setup(x => x.Priority).Returns(100);
        provider1Mock.Setup(x => x.GetTenantContext()).Returns((ITenantContext?)null);

        var provider2Mock = new Mock<ITenantContextProvider>();
        provider2Mock.Setup(x => x.Priority).Returns(50);
        provider2Mock.Setup(x => x.GetTenantContext()).Returns((ITenantContext?)null);

        var resolver = new TenantContextResolver(
            new[] { provider1Mock.Object, provider2Mock.Object },
            NullLogger<TenantContextResolver>.Instance);

        // Act
        var result = resolver.Resolve();

        // Assert
        result.ShouldBe(TenantContext.Empty);
        result.Name.ShouldBeNull();
        result.StorageIdentifier.ShouldBeNull();
    }
}
