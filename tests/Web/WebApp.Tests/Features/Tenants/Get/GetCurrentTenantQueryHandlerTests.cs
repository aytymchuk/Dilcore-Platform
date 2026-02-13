using Dilcore.Tenancy.Contracts.Tenants;
using Dilcore.WebApi.Client.Clients;
using Dilcore.WebApp.Features.Tenants.Get;
using Dilcore.WebApp.Models.Tenants;
using Moq;
using Shouldly;

namespace Dilcore.WebApp.Tests.Features.Tenants.Get;

public class GetCurrentTenantQueryHandlerTests
{
    private readonly Mock<ITenancyClient> _tenancyClientMock;
    private readonly GetCurrentTenantQueryHandler _handler;

    public GetCurrentTenantQueryHandlerTests()
    {
        _tenancyClientMock = new Mock<ITenancyClient>();
        _handler = new GetCurrentTenantQueryHandler(_tenancyClientMock.Object);
    }

    [Test]
    public async Task Handle_Should_Return_Tenant_When_Context_Is_Available()
    {
        // Arrange
        var expectedTenant = new TenantDto 
        { 
            Id = Guid.NewGuid(), 
            SystemName = "current-tenant", 
            Name = "Current Tenant",
            StorageIdentifier = "storage-prefix",
            Description = "Test Description",
            CreatedAt = DateTime.UtcNow
        };

        _tenancyClientMock
            .Setup(x => x.GetTenantAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedTenant);

        var query = new GetCurrentTenantQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Id.ShouldBe(expectedTenant.Id);
        result.Value.SystemName.ShouldBe(expectedTenant.SystemName);
        result.Value.Name.ShouldBe(expectedTenant.Name);
        result.Value.StorageIdentifier.ShouldBe(expectedTenant.StorageIdentifier);
        result.Value.Description.ShouldBe(expectedTenant.Description);
        result.Value.CreatedAt.ShouldBe(expectedTenant.CreatedAt);
    }

    [Test]
    public async Task Handle_Should_Return_Fail_When_Context_Is_Missing()
    {
        // Arrange
        _tenancyClientMock
            .Setup(x => x.GetTenantAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("API Error"));

        var query = new GetCurrentTenantQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsFailed.ShouldBeTrue();
    }
}
