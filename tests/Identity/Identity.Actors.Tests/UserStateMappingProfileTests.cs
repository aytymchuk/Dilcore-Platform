using AutoMapper;
using Dilcore.Identity.Actors.Abstractions;
using Dilcore.Identity.Actors.Profiles;
using Dilcore.Identity.Domain;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Shouldly;
using DomainTenantAccess = Dilcore.Identity.Domain.TenantAccess;
using ActorsTenantAccess = Dilcore.Identity.Actors.Abstractions.TenantAccess;

namespace Dilcore.Identity.Actors.Tests;

/// <summary>
/// Tests for UserStateMappingProfile AutoMapper configuration.
/// </summary>
[TestFixture]
public class UserStateMappingProfileTests
{
    private IMapper _mapper = null!;

    [SetUp]
    public void SetUp()
    {
        var loggerFactory = new LoggerFactory([NullLoggerProvider.Instance]);
        
        var configuration = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<UserStateMappingProfile>();
        }, loggerFactory);

        _mapper = configuration.CreateMapper();
    }

    [Test]
    public void Configuration_ShouldBeValid()
    {
        var loggerFactory = new LoggerFactory([NullLoggerProvider.Instance]);
        
        var configuration = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<UserStateMappingProfile>();
        }, loggerFactory);

        configuration.AssertConfigurationIsValid();
    }

    [Test]
    public void Map_UserToUserState_ShouldMapAllProperties()
    {
        // Arrange
        var user = User.Create(
            identityId: "auth0|user1",
            email: "user1@example.com",
            firstName: "Actor",
            lastName: "Tester",
            timeProvider: TimeProvider.System) with
        {
            Tenants = new List<DomainTenantAccess>
            {
                new() { TenantId = "tenant-actor", Roles = ["editor"] }
            }
        };

        // Act
        var userState = _mapper.Map<UserState>(user);

        // Assert
        userState.Id.ShouldBe(user.Id);
        userState.ETag.ShouldBe(user.ETag);
        userState.IdentityId.ShouldBe(user.IdentityId);
        userState.Email.ShouldBe(user.Email);
        userState.FirstName.ShouldBe(user.FirstName);
        userState.LastName.ShouldBe(user.LastName);
        userState.RegisteredAt.ShouldBe(user.CreatedAt);
        userState.UpdatedAt.ShouldBe(user.UpdatedAt);
        userState.IsRegistered.ShouldBeTrue();
        
        userState.Tenants.ShouldNotBeNull();
        userState.Tenants.Count.ShouldBe(1);
        userState.Tenants[0].TenantId.ShouldBe("tenant-actor");
        userState.Tenants[0].Roles.ShouldBe(["editor"]);
    }

    [Test]
    public void Map_UserStateToUser_ShouldMapAllProperties()
    {
        // Arrange
        var userState = new UserState
        {
            Id = Guid.CreateVersion7(),
            ETag = 999L,
            IdentityId = "auth0|state1",
            Email = "state@example.com",
            FirstName = "State",
            LastName = "User",
            RegisteredAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow.AddHours(1),
            IsRegistered = true,
            Tenants = new List<ActorsTenantAccess>
            {
                new() { TenantId = "tenant-state", Roles = ["admin"] }
            }
        };

        // Act
        var user = _mapper.Map<User>(userState);

        // Assert
        user.Id.ShouldBe(userState.Id);
        user.ETag.ShouldBe(userState.ETag);
        user.IdentityId.ShouldBe(userState.IdentityId);
        user.Email.ShouldBe(userState.Email);
        user.FirstName.ShouldBe(userState.FirstName);
        user.LastName.ShouldBe(userState.LastName);
        user.CreatedAt.ShouldBe(userState.RegisteredAt);
        user.UpdatedAt.ShouldBe(userState.UpdatedAt!.Value);
        
        user.Tenants.ShouldNotBeNull();
        user.Tenants.Count.ShouldBe(1);
        user.Tenants[0].TenantId.ShouldBe("tenant-state");
        user.Tenants[0].Roles.ShouldBe(["admin"]);
    }
}
