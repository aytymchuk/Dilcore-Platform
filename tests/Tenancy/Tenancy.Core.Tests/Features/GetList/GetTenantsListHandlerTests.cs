using Dilcore.Authentication.Abstractions;
using Dilcore.Identity.Actors.Abstractions;
using Dilcore.Results.Abstractions;
using Dilcore.Tenancy.Actors.Abstractions;
using Dilcore.Tenancy.Core.Abstractions;
using Dilcore.Tenancy.Core.Features.GetList;
using Dilcore.Tenancy.Domain;
using FluentResults;
using Moq;
using Shouldly;

namespace Dilcore.Tenancy.Core.Tests.Features.GetList;

/// <summary>
/// Unit tests for GetTenantsListHandler using mocked dependencies.
/// </summary>
[TestFixture]
public class GetTenantsListHandlerTests
{
    private Mock<IGrainFactory> _grainFactoryMock = null!;
    private Mock<IUserContext> _userContextMock = null!;
    private Mock<ITenantRepository> _tenantRepositoryMock = null!;
    private GetTenantsListHandler _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _grainFactoryMock = new Mock<IGrainFactory>();
        _userContextMock = new Mock<IUserContext>();
        _tenantRepositoryMock = new Mock<ITenantRepository>();
        _sut = new GetTenantsListHandler(
            _grainFactoryMock.Object,
            _userContextMock.Object,
            _tenantRepositoryMock.Object);
    }

    [Test]
    public async Task Handle_ShouldReturnEmptyList_WhenUserHasNoTenants()
    {
        // Arrange
        const string userId = "test-user-123";
        _userContextMock.Setup(x => x.Id).Returns(userId);

        var userGrainMock = new Mock<IUserGrain>();
        userGrainMock.Setup(x => x.GetTenantsAsync()).ReturnsAsync(Array.Empty<TenantAccess>());

        _grainFactoryMock.Setup(x => x.GetGrain<IUserGrain>(userId, null)).Returns(userGrainMock.Object);

        var query = new GetTenantsListQuery();

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        var tenants = result.ShouldBeSuccessWithValue();
        tenants.ShouldBeEmpty();
        _tenantRepositoryMock.Verify(x => x.GetBySystemNamesAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task Handle_ShouldReturnSingleTenant_WhenUserHasOneTenant()
    {
        // Arrange
        const string userId = "test-user-123";
        const string tenantSystemName = "test-tenant";
        _userContextMock.Setup(x => x.Id).Returns(userId);

        var tenantAccess = new TenantAccess { TenantId = tenantSystemName };
        var userGrainMock = new Mock<IUserGrain>();
        userGrainMock.Setup(x => x.GetTenantsAsync()).ReturnsAsync(new[] { tenantAccess });

        _grainFactoryMock.Setup(x => x.GetGrain<IUserGrain>(userId, null)).Returns(userGrainMock.Object);

        var tenant = new Tenant
        {
            Id = Guid.CreateVersion7(),
            Name = "Test Tenant",
            SystemName = tenantSystemName,
            Description = "Description",
            StoragePrefix = "storage-prefix",
            CreatedAt = DateTime.UtcNow,
            CreatedById = userId
        };

        _tenantRepositoryMock
            .Setup(x => x.GetBySystemNamesAsync(It.Is<IEnumerable<string>>(names => names.Contains(tenantSystemName)), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok<IReadOnlyList<Tenant>>(new[] { tenant }));

        var query = new GetTenantsListQuery();

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        var tenants = result.ShouldBeSuccessWithValue();
        tenants.ShouldHaveSingleItem();
        tenants[0].SystemName.ShouldBe(tenantSystemName);
        tenants[0].Name.ShouldBe("Test Tenant");
    }

    [Test]
    public async Task Handle_ShouldReturnMultipleTenants_WhenUserHasMultipleTenants()
    {
        // Arrange
        const string userId = "test-user-123";
        const string tenant1SystemName = "tenant-one";
        const string tenant2SystemName = "tenant-two";
        const string tenant3SystemName = "tenant-three";

        _userContextMock.Setup(x => x.Id).Returns(userId);

        var tenantAccesses = new[]
        {
            new TenantAccess { TenantId = tenant1SystemName },
            new TenantAccess { TenantId = tenant2SystemName },
            new TenantAccess { TenantId = tenant3SystemName }
        };

        var userGrainMock = new Mock<IUserGrain>();
        userGrainMock.Setup(x => x.GetTenantsAsync()).ReturnsAsync(tenantAccesses);

        _grainFactoryMock.Setup(x => x.GetGrain<IUserGrain>(userId, null)).Returns(userGrainMock.Object);

        var tenants = new[]
        {
            new Tenant { Id = Guid.CreateVersion7(), Name = "Tenant One", SystemName = tenant1SystemName, Description = "First", StoragePrefix = "prefix1", CreatedAt = DateTime.UtcNow, CreatedById = userId },
            new Tenant { Id = Guid.CreateVersion7(), Name = "Tenant Two", SystemName = tenant2SystemName, Description = "Second", StoragePrefix = "prefix2", CreatedAt = DateTime.UtcNow, CreatedById = userId },
            new Tenant { Id = Guid.CreateVersion7(), Name = "Tenant Three", SystemName = tenant3SystemName, Description = "Third", StoragePrefix = "prefix3", CreatedAt = DateTime.UtcNow, CreatedById = userId }
        };

        _tenantRepositoryMock
            .Setup(x => x.GetBySystemNamesAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok<IReadOnlyList<Tenant>>(tenants));

        var query = new GetTenantsListQuery();

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        var tenantDtos = result.ShouldBeSuccessWithValue();
        tenantDtos.Count.ShouldBe(3);
        tenantDtos[0].SystemName.ShouldBe(tenant1SystemName);
        tenantDtos[1].SystemName.ShouldBe(tenant2SystemName);
        tenantDtos[2].SystemName.ShouldBe(tenant3SystemName);
    }

    [Test]
    public async Task Handle_ShouldReturnFailure_WhenRepositoryFails()
    {
        // Arrange
        const string userId = "test-user-123";
        const string tenantSystemName = "test-tenant";
        _userContextMock.Setup(x => x.Id).Returns(userId);

        var tenantAccess = new TenantAccess { TenantId = tenantSystemName };
        var userGrainMock = new Mock<IUserGrain>();
        userGrainMock.Setup(x => x.GetTenantsAsync()).ReturnsAsync(new[] { tenantAccess });

        _grainFactoryMock.Setup(x => x.GetGrain<IUserGrain>(userId, null)).Returns(userGrainMock.Object);

        var repositoryError = new Error("Repository error");
        _tenantRepositoryMock
            .Setup(x => x.GetBySystemNamesAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Fail<IReadOnlyList<Tenant>>(repositoryError));

        var query = new GetTenantsListQuery();

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.ShouldBeFailed();
        result.Errors.ShouldContain(repositoryError);
    }

    [Test]
    public async Task Handle_ShouldMapPropertiesCorrectly_ToTenantDto()
    {
        // Arrange
        const string userId = "test-user-123";
        const string tenantSystemName = "test-tenant";
        var tenantId = Guid.CreateVersion7();
        const string tenantName = "Test Tenant";
        const string description = "Test Description";
        const string storagePrefix = "test-storage";
        var createdAt = DateTime.UtcNow;
        const string createdById = "creator-123";

        _userContextMock.Setup(x => x.Id).Returns(userId);

        var tenantAccess = new TenantAccess { TenantId = tenantSystemName };
        var userGrainMock = new Mock<IUserGrain>();
        userGrainMock.Setup(x => x.GetTenantsAsync()).ReturnsAsync(new[] { tenantAccess });

        _grainFactoryMock.Setup(x => x.GetGrain<IUserGrain>(userId, null)).Returns(userGrainMock.Object);

        var tenant = new Tenant
        {
            Id = tenantId,
            Name = tenantName,
            SystemName = tenantSystemName,
            Description = description,
            StoragePrefix = storagePrefix,
            CreatedAt = createdAt,
            CreatedById = createdById
        };

        _tenantRepositoryMock
            .Setup(x => x.GetBySystemNamesAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok<IReadOnlyList<Tenant>>(new[] { tenant }));

        var query = new GetTenantsListQuery();

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        var tenantDtos = result.ShouldBeSuccessWithValue();
        var dto = tenantDtos.ShouldHaveSingleItem();
        dto.Id.ShouldBe(tenantId);
        dto.Name.ShouldBe(tenantName);
        dto.SystemName.ShouldBe(tenantSystemName);
        dto.Description.ShouldBe(description);
        dto.StoragePrefix.ShouldBe(storagePrefix);
        dto.CreatedAt.ShouldBe(createdAt);
        dto.CreatedById.ShouldBe(createdById);
    }

    [Test]
    public async Task Handle_ShouldUseUserContextId_ToGetUserGrain()
    {
        // Arrange
        const string userId = "specific-user-id";
        _userContextMock.Setup(x => x.Id).Returns(userId);

        var userGrainMock = new Mock<IUserGrain>();
        userGrainMock.Setup(x => x.GetTenantsAsync()).ReturnsAsync(Array.Empty<TenantAccess>());

        _grainFactoryMock.Setup(x => x.GetGrain<IUserGrain>(userId, null)).Returns(userGrainMock.Object);

        var query = new GetTenantsListQuery();

        // Act
        await _sut.Handle(query, CancellationToken.None);

        // Assert
        _grainFactoryMock.Verify(x => x.GetGrain<IUserGrain>(userId, null), Times.Once);
    }
}
