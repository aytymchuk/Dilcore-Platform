using Dilcore.Common.Domain.Abstractions;
using Dilcore.Common.Domain.Abstractions.Extensions;
using Moq;
using Shouldly;

namespace Dilcore.Common.Domain.Abstractions.Tests;

public class BaseDomainTests
{
    private record TestDomain : BaseDomain;
    private Mock<TimeProvider> _timeProviderMock;
    private DateTimeOffset _fixedTime;

    [SetUp]
    public void SetUp()
    {
        _fixedTime = DateTimeOffset.UtcNow;
        _timeProviderMock = new Mock<TimeProvider>();
        _timeProviderMock.Setup(x => x.GetUtcNow()).Returns(_fixedTime);
    }

    [Test]
    public void UpdateETag_ShouldUpdateValue_AndReturnNewInstance()
    {
        // Arrange
        var domain = new TestDomain();
        var initialETag = domain.ETag;

        // Act
        var updatedDomain = domain.UpdateETag(_timeProviderMock.Object);

        // Assert
        updatedDomain.ShouldNotBeSameAs(domain);
        updatedDomain.ETag.ShouldNotBe(initialETag);
        updatedDomain.ETag.ShouldBe(_fixedTime.ToUnixTimeMilliseconds());
        domain.ETag.ShouldBe(initialETag);
    }

    [Test]
    public void SetCreatedOn_ShouldUpdateValue_AndReturnNewInstance()
    {
        // Arrange
        var domain = new TestDomain();

        // Act
        var updatedDomain = domain.SetCreatedOn(_timeProviderMock.Object);

        // Assert
        updatedDomain.ShouldNotBeSameAs(domain);
        updatedDomain.CreatedOn.ShouldBe(_fixedTime.UtcDateTime);
        domain.CreatedOn.ShouldBe(default);
    }

    [Test]
    public void SetUpdatedOn_ShouldUpdateValue_AndReturnNewInstance()
    {
        // Arrange
        var domain = new TestDomain();

        // Act
        var updatedDomain = domain.SetUpdatedOn(_timeProviderMock.Object);

        // Assert
        updatedDomain.ShouldNotBeSameAs(domain);
        updatedDomain.UpdatedOn.ShouldBe(_fixedTime.UtcDateTime);
        domain.UpdatedOn.ShouldBeNull();
    }
}