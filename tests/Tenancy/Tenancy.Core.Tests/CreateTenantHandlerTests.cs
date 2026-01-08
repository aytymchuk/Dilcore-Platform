using Dilcore.MultiTenant.Abstractions;
using Dilcore.Tenancy.Actors.Abstractions;
using Dilcore.Tenancy.Core.Features.Create;
using Moq;
using Shouldly;

namespace Dilcore.Tenancy.Core.Tests;

/// <summary>
/// Unit tests for CreateTenantHandler using mocked grain factory.
/// </summary>
[TestFixture]
public class CreateTenantHandlerTests
{
    private Mock<IGrainFactory> _grainFactoryMock = null!;
    private CreateTenantHandler _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _grainFactoryMock = new Mock<IGrainFactory>();
        _sut = new CreateTenantHandler(_grainFactoryMock.Object);
    }

    [Test]
    public async Task Handle_ShouldCallGrain_WithKebabCaseName()
    {
        // Arrange
        const string displayName = "My Test Tenant";
        const string expectedKebabName = "my-test-tenant";
        const string description = "A test tenant";

        var expectedDto = new TenantDto(expectedKebabName, displayName, description, DateTime.UtcNow);
        var tenantGrainMock = new Mock<ITenantGrain>();
        tenantGrainMock.Setup(x => x.CreateAsync(displayName, description)).ReturnsAsync(expectedDto);

        _grainFactoryMock.Setup(x => x.GetGrain<ITenantGrain>(expectedKebabName, null)).Returns(tenantGrainMock.Object);

        var command = new CreateTenantCommand(displayName, description);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        var tenant = result.ShouldBeSuccessWithValue();
        tenant.Name.ShouldBe(expectedKebabName);
        tenantGrainMock.Verify(x => x.CreateAsync(displayName, description), Times.Once);
    }

    [Test]
    public async Task Handle_ShouldConvertDisplayName_ToKebabCase()
    {
        // Arrange
        const string displayName = "UPPERCASE With Spaces";
        const string expectedKebabName = "uppercase-with-spaces";
        const string description = "Test";

        var expectedDto = new TenantDto(expectedKebabName, displayName, description, DateTime.UtcNow);
        var tenantGrainMock = new Mock<ITenantGrain>();
        tenantGrainMock.Setup(x => x.CreateAsync(displayName, description)).ReturnsAsync(expectedDto);

        _grainFactoryMock.Setup(x => x.GetGrain<ITenantGrain>(expectedKebabName, null)).Returns(tenantGrainMock.Object);

        var command = new CreateTenantCommand(displayName, description);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.ShouldBeSuccess();
        _grainFactoryMock.Verify(x => x.GetGrain<ITenantGrain>(expectedKebabName, null), Times.Once);
    }

    [Test]
    public async Task Handle_ShouldReturnTenantDto_FromGrain()
    {
        // Arrange
        const string displayName = "Return Test";
        const string description = "Description";
        var createdAt = DateTime.UtcNow;

        var expectedDto = new TenantDto("return-test", displayName, description, createdAt);
        var tenantGrainMock = new Mock<ITenantGrain>();
        tenantGrainMock.Setup(x => x.CreateAsync(displayName, description)).ReturnsAsync(expectedDto);

        _grainFactoryMock.Setup(x => x.GetGrain<ITenantGrain>("return-test", null)).Returns(tenantGrainMock.Object);

        var command = new CreateTenantCommand(displayName, description);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Value.CreatedAt.ShouldBe(createdAt);
        result.Value.DisplayName.ShouldBe(displayName);
        result.Value.Description.ShouldBe(description);
    }
}
