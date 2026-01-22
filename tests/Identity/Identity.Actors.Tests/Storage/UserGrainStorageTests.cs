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
        var userState = new UserState { Id = Guid.NewGuid(), IdentityId = IdentityId };
        _grainStateMock.SetupGet(x => x.State).Returns(userState);
        _grainStateMock.SetupGet(x => x.ETag).Returns("123");

        _userRepositoryMock.Setup(x => x.DeleteByIdentityIdAsync(IdentityId, 123, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok(true));

        // Act
        await _storage.ClearStateAsync("UserStore", _grainId, _grainStateMock.Object);

        // Assert
        _userRepositoryMock.Verify(x => x.DeleteByIdentityIdAsync(IdentityId, 123, It.IsAny<CancellationToken>()), Times.Once);
        _grainStateMock.VerifySet(x => x.RecordExists = false);
        _grainStateMock.VerifySet(x => x.ETag = null);
        _scopeFactoryMock.Verify(x => x.CreateScope(), Times.Once);
    }

    [Test]
    public async Task ReadStateAsync_ShouldThrow_WhenRepositoryFails()
    {
        // Arrange
        _userRepositoryMock.Setup(x => x.GetByIdentityIdAsync(IdentityId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Fail("DB error"));

        // Act & Assert
        Assert.ThrowsAsync<InvalidOperationException>(() =>
            _storage.ReadStateAsync("UserStore", _grainId, _grainStateMock.Object));

        // Verify scope was created before failure
        await Task.Delay(10); // Allow async to execute
        _scopeFactoryMock.Verify(x => x.CreateScope(), Times.Once);
    }

    [Test]
    public async Task WriteStateAsync_ShouldThrow_WhenRepositoryFails()
    {
        // Arrange
        var userState = new UserState { IdentityId = IdentityId };
        _grainStateMock.SetupGet(x => x.State).Returns(userState);
        _mapperMock.Setup(x => x.Map<User>(userState)).Returns(User.Create(IdentityId, "a@b.com", "F", "L", TimeProvider.System));

        _userRepositoryMock.Setup(x => x.StoreAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Fail("DB error"));

        // Act & Assert
        Assert.ThrowsAsync<InvalidOperationException>(() =>
            _storage.WriteStateAsync("UserStore", _grainId, _grainStateMock.Object));

        // Verify scope was created and RecordExists/ETag not set on failure
        await Task.Delay(10); // Allow async to execute
        _scopeFactoryMock.Verify(x => x.CreateScope(), Times.Once);
        _grainStateMock.VerifySet(x => x.RecordExists = true, Times.Never);
        _grainStateMock.VerifySet(x => x.ETag = It.IsAny<string>(), Times.Never);
    }

    [Test]
    public async Task ClearStateAsync_ShouldNotResetState_WhenEtagMismatch()
    {
        // Arrange
        var userState = new UserState { IdentityId = IdentityId };
        _grainStateMock.SetupGet(x => x.State).Returns(userState);
        _grainStateMock.SetupGet(x => x.ETag).Returns("123");

        _userRepositoryMock.Setup(x => x.DeleteByIdentityIdAsync(IdentityId, 123, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok(false)); // ETag mismatch

        // Act
        await _storage.ClearStateAsync("UserStore", _grainId, _grainStateMock.Object);

        // Assert
        _grainStateMock.VerifySet(x => x.State = It.IsAny<object>(), Times.Never);
        _grainStateMock.VerifySet(x => x.RecordExists = false, Times.Never);
        _grainStateMock.VerifySet(x => x.ETag = null, Times.Never);
        _scopeFactoryMock.Verify(x => x.CreateScope(), Times.Once);
        _userRepositoryMock.Verify(x => x.DeleteByIdentityIdAsync(IdentityId, 123, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public void WriteStateAsync_ShouldThrow_WhenStateIsInvalidType()
    {
        // Arrange
        _grainStateMock.SetupGet(x => x.State).Returns(new object()); // Invalid type

        // Act & Assert
        Assert.ThrowsAsync<InvalidOperationException>(() =>
            _storage.WriteStateAsync("UserStore", _grainId, _grainStateMock.Object));
    }

    [Test]
    public void ClearStateAsync_ShouldThrow_WhenStateIsInvalidType()
    {
        // Arrange
        _grainStateMock.SetupGet(x => x.State).Returns(new object()); // Invalid type

        // Act & Assert
        Assert.ThrowsAsync<InvalidOperationException>(() =>
            _storage.ClearStateAsync("UserStore", _grainId, _grainStateMock.Object));
    }

    [Test]
    public void ClearStateAsync_ShouldThrow_WhenRepositoryFails()
    {
        // Arrange
        var userState = new UserState { IdentityId = IdentityId };
        _grainStateMock.SetupGet(x => x.State).Returns(userState);
        _grainStateMock.SetupGet(x => x.ETag).Returns("123");

        _userRepositoryMock.Setup(x => x.DeleteByIdentityIdAsync(IdentityId, 123, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Fail("DB error"));

        // Act & Assert
        Assert.ThrowsAsync<InvalidOperationException>(() =>
            _storage.ClearStateAsync("UserStore", _grainId, _grainStateMock.Object));
    }
}
