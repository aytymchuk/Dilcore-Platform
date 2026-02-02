using Dilcore.DocumentDb.MongoDb.Repositories.Abstractions;
using Dilcore.Tenancy.Domain;
using Dilcore.Tenancy.Store.Entities;
using Dilcore.Tenancy.Store.Repositories;
using FluentResults;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Moq;
using Shouldly;

namespace Dilcore.Tenancy.Store.Tests;

[TestFixture]
public class TenantRepositoryTests
{
    private Mock<IGenericRepository<TenantDocument>>? _genericRepositoryMock;
    private Mock<ILogger<TenantRepository>>? _loggerMock;
    private TenantRepository? _sut;

    [SetUp]
    public void SetUp()
    {
        _genericRepositoryMock = new Mock<IGenericRepository<TenantDocument>>();
        _loggerMock = new Mock<ILogger<TenantRepository>>();
        _sut = new TenantRepository(_genericRepositoryMock.Object, _loggerMock.Object);
    }

    [Test]
    public async Task StoreAsync_ShouldSucceed_WhenRepositorySucceeds()
    {
        // Arrange
        var tenant = new Tenant
        {
            Id = Guid.NewGuid(),
            Name = "Test Tenant",
            SystemName = "test-tenant",
            StoragePrefix = "prefix"
        };
        
        var tenantDoc = TenantDocument.FromDomain(tenant);
        _genericRepositoryMock!.Setup(x => x.StoreAsync(It.IsAny<TenantDocument>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok(tenantDoc)); 

        // Act
        var result = await _sut!.StoreAsync(tenant);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Id.ShouldBe(tenant.Id);
        result.Value.Name.ShouldBe(tenant.Name);
        result.Value.SystemName.ShouldBe(tenant.SystemName);
        _genericRepositoryMock.Verify(x => x.StoreAsync(It.Is<TenantDocument>(d => d.Id == tenant.Id), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task StoreAsync_ShouldFail_WhenRepositoryFails()
    {
        // Arrange
        var tenant = new Tenant { Name = "Fail", SystemName = "fail", StoragePrefix = "fail" };
        var expectedError = new Error("DB Error");

        _genericRepositoryMock!.Setup(x => x.StoreAsync(It.IsAny<TenantDocument>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Fail<TenantDocument>(expectedError));

        // Act
        var result = await _sut!.StoreAsync(tenant);

        // Assert
        result.IsFailed.ShouldBeTrue();
    }

    [Test]
    public async Task GetBySystemNameAsync_ShouldReturnTenant_WhenFound()
    {
        // Arrange
        var systemName = "found-tenant";
        var tenantDoc = new TenantDocument
        {
            Id = Guid.NewGuid(),
            SystemName = systemName,
            Name = "Found Tenant",
            StoragePrefix = "prefix",
            IsDeleted = false
        };

        _genericRepositoryMock!.Setup(x => x.GetAsync(It.IsAny<FilterDefinition<TenantDocument>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok(tenantDoc));

        // Act
        var result = await _sut!.GetBySystemNameAsync(systemName);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value!.SystemName.ShouldBe(systemName);
    }

    [Test]
    public async Task GetBySystemNameAsync_ShouldReturnNull_WhenNotFound()
    {
        // Arrange
        var systemName = "missing-tenant";

        _genericRepositoryMock!.Setup(x => x.GetAsync(It.IsAny<FilterDefinition<TenantDocument>>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(Result.Ok<TenantDocument>(null!));

        // Act
        var result = await _sut!.GetBySystemNameAsync(systemName);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBeNull();
    }
}
