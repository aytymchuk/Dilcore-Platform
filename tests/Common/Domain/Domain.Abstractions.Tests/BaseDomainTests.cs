using Dilcore.Common.Domain.Abstractions;
using Dilcore.Common.Domain.Abstractions.Extensions;
using Moq;
using Shouldly;

namespace Dilcore.Common.Domain.Abstractions.Tests;

public class BaseDomainTests
{
    private record TestDomain : BaseDomain;
    private Mock<TimeProvider> _timeProviderMock;

    [SetUp]
    public void SetUp()
    {
        _timeProviderMock = new Mock<TimeProvider>();
        _timeProviderMock.Setup(x => x.GetUtcNow()).Returns(DateTimeOffset.UtcNow);
    }

    [Test]
    public void UpdateETag_ShouldUpdateValue_AndReturnNewInstance()
    {
        // Arrange
        var domain = new TestDomain();
        var initialETag = domain.ETag;
        var now = DateTimeOffset.UtcNow;
        _timeProviderMock.Setup(x => x.GetUtcNow()).Returns(now);

        // Act
        var updatedDomain = domain.UpdateETag(_timeProviderMock.Object);

        // Assert
        updatedDomain.ShouldNotBeSameAs(domain);
        updatedDomain.ETag.ShouldNotBe(initialETag);
        updatedDomain.ETag.ShouldBe(now.ToUnixTimeMilliseconds());
        domain.ETag.ShouldBe(initialETag);
    }

    [Test]
    public void SetCreatedOn_ShouldUpdateValue_AndReturnNewInstance()
    {
        // Arrange
        var domain = new TestDomain();
        var now = DateTimeOffset.UtcNow;
        _timeProviderMock.Setup(x => x.GetUtcNow()).Returns(now);

        // Act
        var updatedDomain = domain.SetCreatedOn(_timeProviderMock.Object);

        // Assert
        updatedDomain.ShouldNotBeSameAs(domain);
        updatedDomain.CreatedOn.ShouldBe(now.UtcDateTime);
        domain.CreatedOn.ShouldBe(default);
    }

    [Test]
    public void SetUpdatedOn_ShouldUpdateValue_AndReturnNewInstance()
    {
        // Arrange
        var domain = new TestDomain();
        var now = DateTimeOffset.UtcNow;
        _timeProviderMock.Setup(x => x.GetUtcNow()).Returns(now);

        // Act
        var updatedDomain = domain.SetUpdatedOn(_timeProviderMock.Object);

        // Assert
        updatedDomain.ShouldNotBeSameAs(domain);
        updatedDomain.UpdatedOn.ShouldBe(now.UtcDateTime);
        domain.UpdatedOn.ShouldBeNull();
    }
}