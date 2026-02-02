using AutoMapper;
using Dilcore.Identity.Domain;
using Dilcore.Identity.Store.Entities;
using Dilcore.Identity.Store.Profiles;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Shouldly;
using DomainTenantAccess = Dilcore.Identity.Domain.TenantAccess;
using StoreTenantAccess = Dilcore.Identity.Store.Entities.TenantAccess;

namespace Dilcore.Identity.Store.Tests;

/// <summary>
/// Tests for UserMappingProfile AutoMapper configuration.
/// </summary>
[TestFixture]
public class UserMappingProfileTests
{
    private IMapper _mapper = null!;

    [SetUp]
    public void SetUp()
    {
        var loggerFactory = new LoggerFactory([NullLoggerProvider.Instance]);
        
        var configuration = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<UserMappingProfile>();
        }, loggerFactory);

        _mapper = configuration.CreateMapper();
    }

    [Test]
    public void Configuration_ShouldBeValid()
    {
        var loggerFactory = new LoggerFactory([NullLoggerProvider.Instance]);
        
        // Assert that the AutoMapper configuration is valid
        var configuration = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<UserMappingProfile>();
        }, loggerFactory);

        configuration.AssertConfigurationIsValid();
    }

    [Test]
    public void Map_UserToUserDocument_ShouldMapAllProperties()
    {
        // Arrange
        var user = User.Create(
            identityId: "auth0|123456",
            email: "test@example.com",
            firstName: "John",
            lastName: "Doe",
            timeProvider: TimeProvider.System) with
        {
            Tenants = new List<DomainTenantAccess>
            {
                new() { TenantId = "tenant1", Roles = ["admin", "user"] }
            }
        };

        // Act
        var document = _mapper.Map<UserDocument>(user);

        // Assert
        document.Id.ShouldBe(user.Id);
        document.ETag.ShouldBe(user.ETag);
        document.IdentityId.ShouldBe(user.IdentityId);
        document.Email.ShouldBe(user.Email);
        document.FirstName.ShouldBe(user.FirstName);
        document.LastName.ShouldBe(user.LastName);
        document.CreatedAt.ShouldBe(user.CreatedAt);
        document.UpdatedAt.ShouldBe(user.UpdatedAt);
        
        document.Tenants.ShouldNotBeNull();
        document.Tenants.Count.ShouldBe(1);
        document.Tenants[0].TenantId.ShouldBe("tenant1");
        document.Tenants[0].Roles.ShouldBe(["admin", "user"], ignoreOrder: true);
    }

    [Test]
    public void Map_UserDocumentToUser_ShouldMapAllProperties()
    {
        // Arrange
        var document = new UserDocument
        {
            Id = Guid.CreateVersion7(),
            ETag = 12345L,
            IdentityId = "auth0|123456",
            Email = "test@example.com",
            FirstName = "Jane",
            LastName = "Smith",
            CreatedAt = new DateTime(2024, 1, 15, 10, 30, 0, DateTimeKind.Utc),
            UpdatedAt = new DateTime(2024, 1, 16, 14, 45, 0, DateTimeKind.Utc),
            IsDeleted = false,
            Tenants = new List<StoreTenantAccess>
            {
                new() { TenantId = "tenant2", Roles = ["viewer"] }
            }
        };

        // Act
        var user = _mapper.Map<User>(document);

        // Assert
        user.Id.ShouldBe(document.Id);
        user.ETag.ShouldBe(document.ETag);
        user.IdentityId.ShouldBe(document.IdentityId);
        user.Email.ShouldBe(document.Email);
        user.FirstName.ShouldBe(document.FirstName);
        user.LastName.ShouldBe(document.LastName);
        user.CreatedAt.ShouldBe(document.CreatedAt);
        user.UpdatedAt.ShouldBe(document.UpdatedAt);
        
        user.Tenants.ShouldNotBeNull();
        user.Tenants.Count.ShouldBe(1);
        user.Tenants[0].TenantId.ShouldBe("tenant2");
        user.Tenants[0].Roles.ShouldBe(["viewer"]);
    }

    [Test]
    public void Map_RoundTrip_ShouldPreserveData()
    {
        // Arrange
        var originalUser = User.Create(
            identityId: "auth0|roundtrip",
            email: "roundtrip@example.com",
            firstName: "Round",
            lastName: "Trip",
            timeProvider: TimeProvider.System);

        // Act
        var document = _mapper.Map<UserDocument>(originalUser);
        var mappedUser = _mapper.Map<User>(document);

        // Assert
        mappedUser.Id.ShouldBe(originalUser.Id);
        mappedUser.ETag.ShouldBe(originalUser.ETag);
        mappedUser.IdentityId.ShouldBe(originalUser.IdentityId);
        mappedUser.Email.ShouldBe(originalUser.Email);
        mappedUser.FirstName.ShouldBe(originalUser.FirstName);
        mappedUser.LastName.ShouldBe(originalUser.LastName);
        mappedUser.CreatedAt.ShouldBe(originalUser.CreatedAt);
        mappedUser.UpdatedAt.ShouldBe(originalUser.UpdatedAt);
    }
}
