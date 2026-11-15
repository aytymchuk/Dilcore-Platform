using Dilcore.MongoDB.Abstractions.Repositories;
using Dilcore.MongoDB.Abstractions.Results;
using Dilcore.Tenancy.Domain;
using Dilcore.Tenancy.Store.Entities;
using Dilcore.Tenancy.Store.Repositories;
using FluentAssertions;
using FluentResults;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Moq;

namespace Dilcore.Tenancy.Store.Tests;

[TestFixture]
public class TenantRepositoryErrorMappingTests
{
    private Mock<IGenericRepository<TenantDocument>> _genericRepository = null!;
    private TenantRepository _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _genericRepository = new Mock<IGenericRepository<TenantDocument>>();
        _sut = new TenantRepository(_genericRepository.Object, Mock.Of<ILogger<TenantRepository>>());
    }

    [Test]
    public async Task GetBySystemNameAsync_WhenDocumentIsMissing_ReturnsNull()
    {
        _genericRepository
            .Setup(x => x.GetAsync(It.IsAny<FilterDefinition<TenantDocument>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Fail<TenantDocument>(new DocumentNotFoundError()));

        var result = await _sut.GetBySystemNameAsync("missing");

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeNull();
    }

    [Test]
    public async Task StoreAsync_WhenRepositoryFails_ReturnsFailure()
    {
        var tenant = new Tenant
        {
            Name = "Fail",
            SystemName = "fail",
            StorageIdentifier = "fail",
            CreatedById = "test-user"
        };
        _genericRepository
            .Setup(x => x.StoreAsync(It.IsAny<TenantDocument>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Fail<TenantDocument>(new Error("DB Error")));

        var result = await _sut.StoreAsync(tenant);

        result.IsFailed.Should().BeTrue();
    }
}
