using Dilcore.Identity.Actors.Abstractions;
using Orleans.TestingHost;
using Shouldly;

namespace Dilcore.Identity.Actors.Tests;

/// <summary>
/// Integration tests for UserGrain using Orleans TestCluster.
/// </summary>
[TestFixture]
public class UserGrainTests
{
    private ClusterFixture _fixture = null!;
    private TestCluster Cluster => _fixture.Cluster;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _fixture = new ClusterFixture();
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        _fixture.Dispose();
    }

    [Test]
    public async Task RegisterAsync_ShouldCreateUser_WhenNotRegistered()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var grain = Cluster.GrainFactory.GetGrain<IUserGrain>(userId);
        const string email = "test@example.com";
        const string fullName = "Test User";

        // Act
        var beforeRegister = DateTime.UtcNow;
        var result = await grain.RegisterAsync(email, fullName);
        var afterRegister = DateTime.UtcNow;

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        result.User.ShouldNotBeNull();

        result.User.Id.ShouldBe(userId);
        result.User.Email.ShouldBe(email);
        result.User.FullName.ShouldBe(fullName);
        result.User.RegisteredAt.ShouldBeGreaterThanOrEqualTo(beforeRegister);
        result.User.RegisteredAt.ShouldBeLessThanOrEqualTo(afterRegister);
    }

    [Test]
    public async Task RegisterAsync_ShouldReturnFailure_WhenAlreadyRegistered()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var grain = Cluster.GrainFactory.GetGrain<IUserGrain>(userId);
        const string email = "existing@example.com";
        const string fullName = "Existing User";

        // First registration
        var firstResult = await grain.RegisterAsync(email, fullName);
        firstResult.IsSuccess.ShouldBeTrue();

        // Act - Try to register again with different data
        var secondResult = await grain.RegisterAsync("different@example.com", "Different User");

        // Assert - Should return failure
        secondResult.ShouldNotBeNull();
        secondResult.IsSuccess.ShouldBeFalse();
        secondResult.ErrorMessage.ShouldNotBeNullOrWhiteSpace();
        secondResult.ErrorMessage.ShouldContain($"User '{userId}' is already registered");
    }

    [Test]
    public async Task GetProfileAsync_ShouldReturnNull_WhenUserNotRegistered()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var grain = Cluster.GrainFactory.GetGrain<IUserGrain>(userId);

        // Act
        var result = await grain.GetProfileAsync();

        // Assert
        result.ShouldBeNull();
    }

    [Test]
    public async Task GetProfileAsync_ShouldReturnUser_WhenRegistered()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var grain = Cluster.GrainFactory.GetGrain<IUserGrain>(userId);
        const string email = "profile@example.com";
        const string fullName = "Profile User";

        // Register first
        var registerResult = await grain.RegisterAsync(email, fullName);
        registerResult.IsSuccess.ShouldBeTrue();

        // Act
        var result = await grain.GetProfileAsync();

        // Assert
        result.ShouldNotBeNull();
        result.Id.ShouldBe(userId);
        result.Email.ShouldBe(email);
        result.FullName.ShouldBe(fullName);
    }

    [Test]
    public async Task UserState_ShouldBeAccessible_FromMultipleReferences()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var grain = Cluster.GrainFactory.GetGrain<IUserGrain>(userId);
        const string email = "persist@example.com";
        const string fullName = "Persist User";

        // Register
        var registerResult = await grain.RegisterAsync(email, fullName);
        registerResult.IsSuccess.ShouldBeTrue();

        // Act - Get a new reference and fetch profile
        var newGrainRef = Cluster.GrainFactory.GetGrain<IUserGrain>(userId);
        var result = await newGrainRef.GetProfileAsync();

        // Assert - Data should persist
        result.ShouldNotBeNull();
        result.Email.ShouldBe(email);
    }
}