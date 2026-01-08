using Dilcore.Authentication.Abstractions;
using Dilcore.Identity.Actors.Abstractions;
using Dilcore.Identity.Core.Features.GetCurrent;
using Dilcore.Results.Abstractions;
using Moq;
using Shouldly;

namespace Dilcore.Identity.Core.Tests;

/// <summary>
/// Unit tests for GetCurrentUserHandler using mocked grain factory.
/// </summary>
[TestFixture]
public class GetCurrentUserHandlerTests
{
    private Mock<IGrainFactory> _grainFactoryMock = null!;
    private Mock<IUserContextResolver> _userContextResolverMock = null!;
    private Mock<IUserContext> _userContextMock = null!;
    private GetCurrentUserHandler _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _grainFactoryMock = new Mock<IGrainFactory>();
        _userContextResolverMock = new Mock<IUserContextResolver>();
        _userContextMock = new Mock<IUserContext>();

        // Setup resolver to return the mocked user context
        _userContextResolverMock.Setup(x => x.Resolve()).Returns(_userContextMock.Object);

        _sut = new GetCurrentUserHandler(_userContextResolverMock.Object, _grainFactoryMock.Object);
    }

    [Test]
    public async Task Handle_ShouldReturnUser_WhenFound()
    {
        // Arrange
        const string userId = "user-123";
        const string email = "test@example.com";
        const string fullName = "Test User";
        var registeredAt = DateTime.UtcNow;

        _userContextMock.Setup(x => x.Id).Returns(userId);

        var expectedDto = new UserDto(userId, email, fullName, registeredAt);
        var userGrainMock = new Mock<IUserGrain>();
        userGrainMock.Setup(x => x.GetProfileAsync()).ReturnsAsync(expectedDto);

        _grainFactoryMock.Setup(x => x.GetGrain<IUserGrain>(userId, null)).Returns(userGrainMock.Object);

        var query = new GetCurrentUserQuery();

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        var user = result.ShouldBeSuccessWithValue();
        user.Id.ShouldBe(userId);
        user.Email.ShouldBe(email);
        user.FullName.ShouldBe(fullName);
        user.RegisteredAt.ShouldBe(registeredAt);
    }

    [Test]
    public async Task Handle_ShouldReturnError_WhenUserNotFound()
    {
        // Arrange
        const string userId = "user-not-found";

        _userContextMock.Setup(x => x.Id).Returns(userId);

        var userGrainMock = new Mock<IUserGrain>();
        userGrainMock.Setup(x => x.GetProfileAsync()).ReturnsAsync((UserDto?)null);

        _grainFactoryMock.Setup(x => x.GetGrain<IUserGrain>(userId, null)).Returns(userGrainMock.Object);

        var query = new GetCurrentUserQuery();

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.ShouldBeFailedWithErrorAndMessage<NotFoundError>("User not found");
    }

    [Test]
    public async Task Handle_ShouldUseUserContextId_ToGetGrain()
    {
        // Arrange
        const string userId = "specific-user-id";

        _userContextMock.Setup(x => x.Id).Returns(userId);

        var userGrainMock = new Mock<IUserGrain>();
        userGrainMock.Setup(x => x.GetProfileAsync()).ReturnsAsync((UserDto?)null);

        _grainFactoryMock.Setup(x => x.GetGrain<IUserGrain>(userId, null)).Returns(userGrainMock.Object);

        var query = new GetCurrentUserQuery();

        // Act
        await _sut.Handle(query, CancellationToken.None);

        // Assert
        _grainFactoryMock.Verify(x => x.GetGrain<IUserGrain>(userId, null), Times.Once);
        _userContextResolverMock.Verify(x => x.Resolve(), Times.Once);
    }
}
