using Dilcore.Results.Abstractions;
using Dilcore.Tenancy.Actors.Abstractions;
using Dilcore.Tenancy.Core.Features.Create;
using Moq;
using Shouldly;
using ActorDto = Dilcore.Tenancy.Actors.Abstractions.TenantDto;

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
        const string storagePrefix = "my-test-tenant";

        var expectedDto = new ActorDto(Guid.CreateVersion7(), displayName, expectedKebabName, description,  storagePrefix, true, DateTime.UtcNow);
        var tenantGrainMock = new Mock<ITenantGrain>();
        tenantGrainMock.Setup(x => x.CreateAsync(It.Is<CreateTenantGrainCommand>(c => c.DisplayName == displayName && c.Description == description)))
            .ReturnsAsync(TenantCreationResult.Success(expectedDto));

        _grainFactoryMock.Setup(x => x.GetGrain<ITenantGrain>(expectedKebabName, null)).Returns(tenantGrainMock.Object);

        var command = new CreateTenantCommand(displayName, description);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        var tenant = result.ShouldBeSuccessWithValue();
        tenant.SystemName.ShouldBe(expectedKebabName);
        tenantGrainMock.Verify(x => x.CreateAsync(It.Is<CreateTenantGrainCommand>(c => c.DisplayName == displayName && c.Description == description)), Times.Once);
    }

    [Test]
    public async Task Handle_ShouldConvertName_ToKebabCase()
    {
        // Arrange
        const string displayName = "UPPERCASE With Spaces";
        const string expectedKebabName = "uppercase-with-spaces";
        const string description = "Test";
        const string storagePrefix = "uppercase-with-spaces";

        var expectedDto = new ActorDto(Guid.CreateVersion7(), displayName, expectedKebabName, description, storagePrefix, true, DateTime.UtcNow);
        var tenantGrainMock = new Mock<ITenantGrain>();
        tenantGrainMock.Setup(x => x.CreateAsync(It.Is<CreateTenantGrainCommand>(c => c.DisplayName == displayName)))
            .ReturnsAsync(TenantCreationResult.Success(expectedDto));

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
        const string storagePrefix = "return-test";
        var createdAt = DateTime.UtcNow;

        var expectedDto = new ActorDto(Guid.CreateVersion7(), displayName, "return-test", description, storagePrefix, true, createdAt);
        var tenantGrainMock = new Mock<ITenantGrain>();
        tenantGrainMock.Setup(x => x.CreateAsync(It.IsAny<CreateTenantGrainCommand>()))
            .ReturnsAsync(TenantCreationResult.Success(expectedDto));

        _grainFactoryMock.Setup(x => x.GetGrain<ITenantGrain>("return-test", null)).Returns(tenantGrainMock.Object);

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

        // Grain returns an existing tenant via GetAsync
        var existingTenant = new ActorDto(Guid.CreateVersion7(), displayName, expectedKebabName, description, expectedKebabName, true, DateTime.UtcNow);
        var tenantGrainMock = new Mock<ITenantGrain>();
        tenantGrainMock.Setup(x => x.GetAsync()).ReturnsAsync(existingTenant);

        _grainFactoryMock.Setup(x => x.GetGrain<ITenantGrain>(expectedKebabName, null)).Returns(tenantGrainMock.Object);

        var command = new CreateTenantCommand(displayName, description);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldContain(e => e is ConflictError);
        tenantGrainMock.Verify(x => x.GetAsync(), Times.Once);
        tenantGrainMock.Verify(x => x.CreateAsync(It.IsAny<CreateTenantGrainCommand>()), Times.Never);
    }

    [Test]
    public async Task Handle_ShouldReturnValidationError_WhenNameIsInvalidAfterNormalization()
    {
        // Arrange
        const string displayName = "!!!"; // Normalizes to empty string
        var command = new CreateTenantCommand(displayName, "Description");

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldContain(e => e is ValidationError && e.Message == "Tenant name is invalid after normalization");
        _grainFactoryMock.Verify(x => x.GetGrain<ITenantGrain>(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }
}