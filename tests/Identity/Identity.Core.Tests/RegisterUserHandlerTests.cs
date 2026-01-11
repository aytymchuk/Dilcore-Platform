using Dilcore.Authentication.Abstractions;
using Dilcore.Identity.Actors.Abstractions;
using Dilcore.Identity.Core.Features.Register;
using Dilcore.Results.Abstractions;
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
    private Mock<IUserContextResolver> _userContextResolverMock = null!;
    private Mock<IUserContext> _userContextMock = null!;
    private RegisterUserHandler _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _grainFactoryMock = new Mock<IGrainFactory>();
        _userContextResolverMock = new Mock<IUserContextResolver>();
        _userContextMock = new Mock<IUserContext>();

        // Setup resolver to return the mocked user context
        _userContextResolverMock.Setup(x => x.Resolve()).Returns(_userContextMock.Object);

        _sut = new RegisterUserHandler(_userContextResolverMock.Object, _grainFactoryMock.Object);
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
        var creationResult = UserCreationResult.Success(expectedDto);
        var userGrainMock = new Mock<IUserGrain>();
        userGrainMock.Setup(x => x.RegisterAsync(email, fullName)).ReturnsAsync(creationResult);

        _grainFactoryMock.Setup(x => x.GetGrain<IUserGrain>(userId, null)).Returns(userGrainMock.Object);

        var command = new RegisterUserCommand(email, fullName);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        var user = result.ShouldBeSuccessWithValue();
        user.Id.ShouldBe(userId);
        user.Email.ShouldBe(email);
        userGrainMock.Verify(x => x.RegisterAsync(email, fullName), Times.Once);
        _userContextResolverMock.Verify(x => x.Resolve(), Times.Once);
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
        var creationResult = UserCreationResult.Success(expectedDto);
        var userGrainMock = new Mock<IUserGrain>();
        userGrainMock.Setup(x => x.RegisterAsync(email, fullName)).ReturnsAsync(creationResult);

        _grainFactoryMock.Setup(x => x.GetGrain<IUserGrain>(userId, null)).Returns(userGrainMock.Object);

        var command = new RegisterUserCommand(email, fullName);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        var user = result.ShouldBeSuccessWithValue();
        user.RegisteredAt.ShouldBe(registeredAt);
    }

    [Test]
    public async Task Handle_ShouldReturnConflict_WhenUserAlreadyRegistered()
    {
        // Arrange
        const string userId = "user-789";
        const string email = "conflict@example.com";
        const string fullName = "Conflict User";
        const string errorMessage = "User already registered";

        _userContextMock.Setup(x => x.Id).Returns(userId);

        var creationResult = UserCreationResult.Failure(errorMessage);
        var userGrainMock = new Mock<IUserGrain>();
        userGrainMock.Setup(x => x.RegisterAsync(email, fullName)).ReturnsAsync(creationResult);

        _grainFactoryMock.Setup(x => x.GetGrain<IUserGrain>(userId, null)).Returns(userGrainMock.Object);

        var command = new RegisterUserCommand(email, fullName);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.ShouldBeFailedWithErrorAndMessage<ConflictError>(errorMessage);
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
        result.ShouldBeFailedWithErrorAndMessage<ValidationError>("User ID is required for registration");
    }
}