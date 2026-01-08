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
        var result = await grain.RegisterAsync(email, fullName);

        // Assert
        result.ShouldNotBeNull();
        result.Id.ShouldBe(userId);
        result.Email.ShouldBe(email);
        result.FullName.ShouldBe(fullName);
        result.RegisteredAt.ShouldBeGreaterThan(DateTime.MinValue);
    }

    [Test]
    public async Task RegisterAsync_ShouldReturnExistingUser_WhenAlreadyRegistered()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var grain = Cluster.GrainFactory.GetGrain<IUserGrain>(userId);
        const string email = "existing@example.com";
        const string fullName = "Existing User";

        // First registration
        var firstResult = await grain.RegisterAsync(email, fullName);

        // Act - Try to register again with different data
        var secondResult = await grain.RegisterAsync("different@example.com", "Different User");

        // Assert - Should return original registration data
        secondResult.ShouldNotBeNull();
        secondResult.Email.ShouldBe(email);
        secondResult.FullName.ShouldBe(fullName);
        secondResult.RegisteredAt.ShouldBe(firstResult.RegisteredAt);
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
        await grain.RegisterAsync(email, fullName);

        // Act
        var result = await grain.GetProfileAsync();

        // Assert
        result.ShouldNotBeNull();
        result.Id.ShouldBe(userId);
        result.Email.ShouldBe(email);
        result.FullName.ShouldBe(fullName);
    }

    [Test]
    public async Task UserState_ShouldPersist_AcrossGrainCalls()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var grain = Cluster.GrainFactory.GetGrain<IUserGrain>(userId);
        const string email = "persist@example.com";
        const string fullName = "Persist User";

        // Register
        await grain.RegisterAsync(email, fullName);

        // Act - Get a new reference and fetch profile
        var newGrainRef = Cluster.GrainFactory.GetGrain<IUserGrain>(userId);
        var result = await newGrainRef.GetProfileAsync();

        // Assert - Data should persist
        result.ShouldNotBeNull();
        result.Email.ShouldBe(email);
    }
}
