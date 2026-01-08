using Dilcore.MultiTenant.Abstractions;
using Dilcore.Results.Abstractions;
using Dilcore.Tenancy.Actors.Abstractions;
using Dilcore.Tenancy.Core.Features.Get;
using Moq;
using Shouldly;

namespace Dilcore.Tenancy.Core.Tests;

/// <summary>
/// Unit tests for GetTenantHandler using mocked dependencies.
/// </summary>
[TestFixture]
public class GetTenantHandlerTests
{
    private Mock<IGrainFactory> _grainFactoryMock = null!;
    private Mock<ITenantContext> _tenantContextMock = null!;
    private GetTenantHandler _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _grainFactoryMock = new Mock<IGrainFactory>();
        _tenantContextMock = new Mock<ITenantContext>();
        _sut = new GetTenantHandler(_tenantContextMock.Object, _grainFactoryMock.Object);
    }

    [Test]
    public async Task Handle_ShouldReturnTenant_WhenFound()
    {
        // Arrange
        const string tenantName = "test-tenant";
        const string displayName = "Test Tenant";
        const string description = "A test tenant";
        var createdAt = DateTime.UtcNow;

        _tenantContextMock.Setup(x => x.Name).Returns(tenantName);

        var expectedDto = new TenantDto(tenantName, displayName, description, createdAt);
        var tenantGrainMock = new Mock<ITenantGrain>();
        tenantGrainMock.Setup(x => x.GetAsync()).ReturnsAsync(expectedDto);

        _grainFactoryMock.Setup(x => x.GetGrain<ITenantGrain>(tenantName, null)).Returns(tenantGrainMock.Object);

        var query = new GetTenantQuery();

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        var tenant = result.ShouldBeSuccessWithValue();
        tenant.Name.ShouldBe(tenantName);
        tenant.DisplayName.ShouldBe(displayName);
    }

    [Test]
    public async Task Handle_ShouldReturnFail_WhenTenantNotFound()
    {
        // Arrange
        const string tenantName = "nonexistent";

        _tenantContextMock.Setup(x => x.Name).Returns(tenantName);

        var tenantGrainMock = new Mock<ITenantGrain>();
        tenantGrainMock.Setup(x => x.GetAsync()).ReturnsAsync((TenantDto?)null);

        _grainFactoryMock.Setup(x => x.GetGrain<ITenantGrain>(tenantName, null)).Returns(tenantGrainMock.Object);

        var query = new GetTenantQuery();

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.ShouldBeFailed();
        result.Errors.ShouldContain(e => e is NotFoundError);
    }

    [Test]
    public async Task Handle_ShouldReturnFail_WhenTenantContextNameIsNull()
    {
        // Arrange
        _tenantContextMock.Setup(x => x.Name).Returns((string?)null);

        var query = new GetTenantQuery();

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.ShouldBeFailedWithMessage("Tenant name is required");
    }

    [Test]
    public async Task Handle_ShouldUseTenantContextName_ToGetGrain()
    {
        // Arrange
        const string tenantName = "specific-tenant";

        _tenantContextMock.Setup(x => x.Name).Returns(tenantName);

        var tenantGrainMock = new Mock<ITenantGrain>();
        tenantGrainMock.Setup(x => x.GetAsync()).ReturnsAsync((TenantDto?)null);

        _grainFactoryMock.Setup(x => x.GetGrain<ITenantGrain>(tenantName, null)).Returns(tenantGrainMock.Object);

        var query = new GetTenantQuery();

        // Act
        await _sut.Handle(query, CancellationToken.None);

        // Assert
        _grainFactoryMock.Verify(x => x.GetGrain<ITenantGrain>(tenantName, null), Times.Once);
    }
}
