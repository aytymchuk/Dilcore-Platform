using Dilcore.Tenancy.Contracts.Tenants;
using Dilcore.WebApi.Client.Clients;
using Dilcore.WebApp.Features.Tenants.Get;
using Moq;
using Shouldly;

namespace Dilcore.WebApp.Tests.Features.Tenants.Get;

public class GetTenantBySystemNameQueryHandlerTests
{
    private readonly Mock<ITenancyClient> _tenancyClientMock;
    private readonly GetTenantBySystemNameQueryHandler _handler;

    public GetTenantBySystemNameQueryHandlerTests()
    {
        _tenancyClientMock = new Mock<ITenancyClient>();
        _handler = new GetTenantBySystemNameQueryHandler(_tenancyClientMock.Object);
    }

    [Test]
    public async Task Handle_Should_Return_Tenant_When_Context_Is_Available()
    {
        // Arrange
        var expectedTenant = new TenantDto { Id = Guid.NewGuid(), SystemName = "current-tenant", Name = "Current Tenant" };

        _tenancyClientMock
            .Setup(x => x.GetTenantAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedTenant);

        var query = new GetTenantBySystemNameQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.SystemName.ShouldBe(expectedTenant.SystemName);
        result.Value.Name.ShouldBe(expectedTenant.Name);
    }

    [Test]
    public async Task Handle_Should_Return_Fail_When_Context_Is_Missing()
    {
        // Arrange
        // Mock Refit ApiException or generic exception to simulate failure in SafeGetTenantAsync wrapper
        // Since we are mocking the interface directly, we can make it throw or assume SafeGetTenantAsync handles it.
        // Wait, SafeGetTenantAsync is an extension method that calls InvokeAsync. 
        // We can't mock the extension method. We mock the interface method it calls.
        // If interface throws, Safe... catches it. 
        
        _tenancyClientMock
            .Setup(x => x.GetTenantAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("API Error"));

        var query = new GetTenantBySystemNameQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsFailed.ShouldBeTrue();
        // The SafeApiInvoker typically wraps exceptions. 
    }
}
