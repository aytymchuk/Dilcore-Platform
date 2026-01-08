using Dilcore.Authentication.Abstractions;
using Dilcore.Identity.Actors.Abstractions;
using Dilcore.Identity.Core.Features.Register;
using Moq;
using Shouldly;

namespace Dilcore.Identity.Core.Tests;

/// <summary>
/// Unit tests for RegisterUserHandler using mocked grain factory.
/// </summary>
[TestFixture]
public class RegisterUserHandlerTests
{
    private Mock<IGrainFactory> _grainFactoryMock = null!;
    private Mock<IUserContext> _userContextMock = null!;
    private RegisterUserHandler _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _grainFactoryMock = new Mock<IGrainFactory>();
        _userContextMock = new Mock<IUserContext>();
        _sut = new RegisterUserHandler(_userContextMock.Object, _grainFactoryMock.Object);
    }

    [Test]
    public async Task Handle_ShouldCallGrain_WithUserContextId()
    {
        // Arrange
        const string userId = "user-123";
        const string email = "test@example.com";
        const string fullName = "Test User";

        _userContextMock.Setup(x => x.Id).Returns(userId);

        var expectedDto = new UserDto(userId, email, fullName, DateTime.UtcNow);
        var userGrainMock = new Mock<IUserGrain>();
        userGrainMock.Setup(x => x.RegisterAsync(email, fullName)).ReturnsAsync(expectedDto);

        _grainFactoryMock.Setup(x => x.GetGrain<IUserGrain>(userId, null)).Returns(userGrainMock.Object);

        var command = new RegisterUserCommand(email, fullName);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.Id.ShouldBe(userId);
        result.Value.Email.ShouldBe(email);
        userGrainMock.Verify(x => x.RegisterAsync(email, fullName), Times.Once);
    }

    [Test]
    public async Task Handle_ShouldReturnUserDto_FromGrain()
    {
        // Arrange
        const string userId = "user-456";
        const string email = "another@example.com";
        const string fullName = "Another User";
        var registeredAt = DateTime.UtcNow;

        _userContextMock.Setup(x => x.Id).Returns(userId);

        var expectedDto = new UserDto(userId, email, fullName, registeredAt);
        var userGrainMock = new Mock<IUserGrain>();
        userGrainMock.Setup(x => x.RegisterAsync(email, fullName)).ReturnsAsync(expectedDto);

        _grainFactoryMock.Setup(x => x.GetGrain<IUserGrain>(userId, null)).Returns(userGrainMock.Object);

        var command = new RegisterUserCommand(email, fullName);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Value.RegisteredAt.ShouldBe(registeredAt);
    }

    [Test]
    public async Task Handle_ShouldReturnFail_WhenUserContextIdIsNull()
    {
        // Arrange
        _userContextMock.Setup(x => x.Id).Returns((string?)null);

        var command = new RegisterUserCommand("test@example.com", "Test User");

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailed.ShouldBeTrue();
        result.Errors.ShouldContain(e => e.Message.Contains("User ID is required"));
    }
}
