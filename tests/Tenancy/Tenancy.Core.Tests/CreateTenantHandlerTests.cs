using Dilcore.Tenancy.Actors.Abstractions;
using Dilcore.Tenancy.Core.Features.Create;
using Moq;
using Shouldly;

using Dilcore.Tenancy.Core.Abstractions;
using Dilcore.Results.Abstractions;
using Dilcore.Tenancy.Domain;
using FluentResults;

namespace Dilcore.Tenancy.Core.Tests;

/// <summary>
/// Unit tests for CreateTenantHandler using mocked grain factory.
/// </summary>
[TestFixture]
public class CreateTenantHandlerTests
{
    private Mock<IGrainFactory> _grainFactoryMock = null!;
    private Mock<ITenantRepository> _tenantRepositoryMock = null!;
    private CreateTenantHandler _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _grainFactoryMock = new Mock<IGrainFactory>();
        _tenantRepositoryMock = new Mock<ITenantRepository>();
        _sut = new CreateTenantHandler(_grainFactoryMock.Object, _tenantRepositoryMock.Object);
    }

    [Test]
    public async Task Handle_ShouldCallGrain_WithKebabCaseName()
    {
        // Arrange
        const string displayName = "My Test Tenant";
        const string expectedKebabName = "my-test-tenant";
        const string description = "A test tenant";

        var expectedDto = new TenantDto(Guid.NewGuid(), displayName, expectedKebabName, description, DateTime.UtcNow);
        var tenantGrainMock = new Mock<ITenantGrain>();
        tenantGrainMock.Setup(x => x.CreateAsync(displayName, description)).ReturnsAsync(TenantCreationResult.Success(expectedDto));

        _grainFactoryMock.Setup(x => x.GetGrain<ITenantGrain>(expectedKebabName, null)).Returns(tenantGrainMock.Object);
        _tenantRepositoryMock.Setup(x => x.GetBySystemNameAsync(expectedKebabName, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok<Tenant?>(null)); // Tenant does not exist

        var command = new CreateTenantCommand(displayName, description);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        var tenant = result.ShouldBeSuccessWithValue();
        tenant.SystemName.ShouldBe(expectedKebabName);
        tenantGrainMock.Verify(x => x.CreateAsync(displayName, description), Times.Once);
        _tenantRepositoryMock.Verify(x => x.GetBySystemNameAsync(expectedKebabName, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task Handle_ShouldConvertName_ToKebabCase()
    {
        // Arrange
        const string displayName = "UPPERCASE With Spaces";
        const string expectedKebabName = "uppercase-with-spaces";
        const string description = "Test";

        var expectedDto = new TenantDto(Guid.NewGuid(), displayName, expectedKebabName, description, DateTime.UtcNow);
        var tenantGrainMock = new Mock<ITenantGrain>();
        tenantGrainMock.Setup(x => x.CreateAsync(displayName, description)).ReturnsAsync(TenantCreationResult.Success(expectedDto));

        _grainFactoryMock.Setup(x => x.GetGrain<ITenantGrain>(expectedKebabName, null)).Returns(tenantGrainMock.Object);
        _tenantRepositoryMock.Setup(x => x.GetBySystemNameAsync(expectedKebabName, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok<Tenant?>(null));

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

        var expectedDto = new TenantDto(Guid.NewGuid(), displayName, "return-test", description, createdAt);
        var tenantGrainMock = new Mock<ITenantGrain>();
        tenantGrainMock.Setup(x => x.CreateAsync(displayName, description)).ReturnsAsync(TenantCreationResult.Success(expectedDto));

        _grainFactoryMock.Setup(x => x.GetGrain<ITenantGrain>("return-test", null)).Returns(tenantGrainMock.Object);
        _tenantRepositoryMock.Setup(x => x.GetBySystemNameAsync("return-test", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok<Tenant?>(null));

        var command = new CreateTenantCommand(displayName, description);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        var tenant = result.ShouldBeSuccessWithValue();
        tenant.CreatedAt.ShouldBe(createdAt);
        tenant.Name.ShouldBe(displayName);
        tenant.Description.ShouldBe(description);
    }

    [Test]
    public async Task Handle_ShouldFail_WhenTenantAlreadyExists()
    {
        // Arrange
        const string displayName = "Existing Tenant";
        const string expectedKebabName = "existing-tenant";
        const string description = "Description";

        // Repository returns an existing tenant
        var existingTenant = new Tenant
        {
            Name = displayName,
            SystemName = expectedKebabName,
            StoragePrefix = "existing-prefix" // Placeholder
        };

        _tenantRepositoryMock.Setup(x => x.GetBySystemNameAsync(expectedKebabName, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok<Tenant?>(existingTenant));

        var command = new CreateTenantCommand(displayName, description);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldContain(e => e is ConflictError);
        _grainFactoryMock.Verify(x => x.GetGrain<ITenantGrain>(It.IsAny<string>(), null), Times.Never);
    }
}
