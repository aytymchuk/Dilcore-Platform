using Moq;
using Shouldly;

namespace Dilcore.Identity.Domain.Tests;

public class UserTests
{
    [Test]
    public void Create_ShouldInitializeUser_WithCorrectValues()
    {
        // Arrange
        var identityId = "auth0|123456";
        var email = "test@example.com";
        var firstName = "John";
        var lastName = "Doe";
        var timeProviderMock = new Mock<TimeProvider>();
        var now = DateTimeOffset.UtcNow;
        timeProviderMock.Setup(x => x.GetUtcNow()).Returns(now);

        // Act
        var user = User.Create(identityId, email, firstName, lastName, timeProviderMock.Object);

        // Assert
        user.ShouldNotBeNull();
        user.Id.ShouldNotBe(Guid.Empty);
        user.IdentityId.ShouldBe(identityId);
        user.Email.ShouldBe(email);
        user.FirstName.ShouldBe(firstName);
        user.LastName.ShouldBe(lastName);
        user.FullName.ShouldBe("John Doe");
        user.CreatedAt.ShouldBe(now.UtcDateTime);
        user.UpdatedAt.ShouldBe(default);
        user.ETag.ShouldNotBe(0);
    }
}