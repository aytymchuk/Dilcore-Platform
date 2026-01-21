using AutoMapper;
using Dilcore.Identity.Actors.Storage;
using Dilcore.Identity.Core.Abstractions;
using Dilcore.Identity.Domain;
using FluentResults;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;

namespace Dilcore.Identity.Actors.Tests.Storage;

[TestFixture]
public class UserGrainStorageTests
{
    private Mock<IServiceScopeFactory> _scopeFactoryMock = null!;
    private Mock<IServiceScope> _scopeMock = null!;
    private Mock<IServiceProvider> _serviceProviderMock = null!;
    private Mock<IUserRepository> _userRepositoryMock = null!;
    private Mock<IMapper> _mapperMock = null!;
    private Mock<ILogger<UserGrainStorage>> _loggerMock = null!;
    private UserGrainStorage _storage = null!;
    private Mock<IGrainState<object>> _grainStateMock = null!;
    private const string IdentityId = "test-identity-id";
    private readonly GrainId _grainId = GrainId.Create("user", IdentityId);

    [SetUp]
    public void SetUp()
    {
        _scopeFactoryMock = new Mock<IServiceScopeFactory>();
        _scopeMock = new Mock<IServiceScope>();
        _serviceProviderMock = new Mock<IServiceProvider>();
        _userRepositoryMock = new Mock<IUserRepository>();
        _mapperMock = new Mock<IMapper>();
        _loggerMock = new Mock<ILogger<UserGrainStorage>>();
        _grainStateMock = new Mock<IGrainState<object>>();

        // Setup Scope Factory Chain
        _scopeFactoryMock.Setup(x => x.CreateScope()).Returns(_scopeMock.Object);
        _scopeMock.Setup(x => x.ServiceProvider).Returns(_serviceProviderMock.Object);
        _serviceProviderMock.Setup(x => x.GetService(typeof(IUserRepository))).Returns(_userRepositoryMock.Object);

        _storage = new UserGrainStorage(_scopeFactoryMock.Object, _mapperMock.Object, _loggerMock.Object);
    }

    [Test]
    public async Task ReadStateAsync_ShouldLoadState_WhenUserExists()
    {
        // Arrange
        var user = User.Create(IdentityId, "test@example.com", "First", "Last", TimeProvider.System);
        var result = Result.Ok<User?>(user);

        _userRepositoryMock.Setup(x => x.GetByIdentityIdAsync(IdentityId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);

        var userState = new UserState();
        _mapperMock.Setup(x => x.Map<object>(user))
            .Returns(userState);

        // Act
        await _storage.ReadStateAsync("UserStore", _grainId, _grainStateMock.Object);

        // Assert
        _grainStateMock.VerifySet(x => x.State = userState);
        _grainStateMock.VerifySet(x => x.RecordExists = true);
        _grainStateMock.VerifySet(x => x.ETag = user.ETag.ToString());
        _scopeFactoryMock.Verify(x => x.CreateScope(), Times.Once); // Verify scope creation
    }

    [Test]
    public async Task ReadStateAsync_ShouldSetDefaultState_WhenUserNotFound()
    {
        // Arrange
        var result = Result.Ok<User?>(null);

        _userRepositoryMock.Setup(x => x.GetByIdentityIdAsync(IdentityId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);

        // Act
        await _storage.ReadStateAsync("UserStore", _grainId, _grainStateMock.Object);

        // Assert
        _grainStateMock.VerifySet(x => x.State = It.IsAny<object>()); // Should be new instance
        _grainStateMock.VerifySet(x => x.RecordExists = false);
        _grainStateMock.VerifySet(x => x.ETag = null);
        _scopeFactoryMock.Verify(x => x.CreateScope(), Times.Once);
    }

    [Test]
    public async Task WriteStateAsync_ShouldStoreUser_WhenStateIsValid()
    {
        // Arrange
        var userState = new UserState
        {
            Id = Guid.NewGuid(),
            IdentityId = IdentityId,
            Email = "test@example.com"
        };
        _grainStateMock.SetupGet(x => x.State).Returns(userState);

        var user = User.Create(IdentityId, "test@example.com", "First", "Last", TimeProvider.System);
        _mapperMock.Setup(x => x.Map<User>(userState))
            .Returns(user);

        var storedUser = user with { ETag = 12345 };
        var result = Result.Ok(storedUser);

        _userRepositoryMock.Setup(x => x.StoreAsync(user, It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);

        // Act
        await _storage.WriteStateAsync("UserStore", _grainId, _grainStateMock.Object);

        // Assert
        _userRepositoryMock.Verify(x => x.StoreAsync(user, It.IsAny<CancellationToken>()), Times.Once);
        _grainStateMock.VerifySet(x => x.RecordExists = true);
        _grainStateMock.VerifySet(x => x.ETag = "12345");
        _scopeFactoryMock.Verify(x => x.CreateScope(), Times.Once);
    }

    [Test]
    public async Task ClearStateAsync_ShouldDeleteUser_WhenCalled()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var userState = new UserState { Id = Guid.NewGuid(), IdentityId = userId };
        _grainStateMock.SetupGet(x => x.State).Returns(userState);
        _grainStateMock.SetupGet(x => x.ETag).Returns("123");

        _userRepositoryMock.Setup(x => x.DeleteByIdentityIdAsync(userId, 123, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok(true));

        // Act
        await _storage.ClearStateAsync("UserStore", _grainId, _grainStateMock.Object);

        // Assert
        _userRepositoryMock.Verify(x => x.DeleteByIdentityIdAsync(userId, 123, It.IsAny<CancellationToken>()), Times.Once);
        _grainStateMock.VerifySet(x => x.RecordExists = false);
        _grainStateMock.VerifySet(x => x.ETag = null);
        _scopeFactoryMock.Verify(x => x.CreateScope(), Times.Once);
    }
}
