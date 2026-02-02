using Shouldly;

namespace Dilcore.MultiTenant.Orleans.Extensions.Tests;

/// <summary>
/// Unit tests for OrleansTenantContextProvider.
/// </summary>
[TestFixture]
public class OrleansTenantContextProviderTests
{
    private OrleansTenantContextProvider _provider = null!;

    [SetUp]
    public void SetUp()
    {
        _provider = new OrleansTenantContextProvider();
        RequestContext.Clear();
    }

    [Test]
    public void Priority_ShouldBe200()
    {
        // Priority must be 200 to ensure Orleans provider is checked before HTTP provider (100)
        // This intentionally covers both the exact contract and relative ordering requirement

        // Act & Assert
        _provider.Priority.ShouldBe(200);
    }

    [Test]
    public void GetTenantContext_ShouldReturnNull_WhenNoContextInRequestContext()
    {
        // Act
        var result = _provider.GetTenantContext();

        // Assert
        result.ShouldBeNull();
    }

    [Test]
    public void GetTenantContext_ShouldReturnContext_WhenContextExistsInRequestContext()
    {
        // Arrange
        RequestContext.Set("TenantContext.Id", Guid.CreateVersion7().ToString());
        RequestContext.Set("TenantContext.Name", "test-tenant");
        RequestContext.Set("TenantContext.StorageIdentifier", "storage-id-123");

        // Act
        var result = _provider.GetTenantContext();

        // Assert
        result.ShouldNotBeNull();
        result.Id.ShouldNotBe(Guid.Empty);
        result.Name.ShouldBe("test-tenant");
        result.StorageIdentifier.ShouldBe("storage-id-123");
    }

    [Test]
    public void GetTenantContext_ShouldReturnContext_WhenOnlyNameExists()
    {
        // Arrange
        RequestContext.Set("TenantContext.Id", Guid.CreateVersion7().ToString());
        RequestContext.Set("TenantContext.Name", "test-tenant");

        // Act
        var result = _provider.GetTenantContext();

        // Assert
        result.ShouldNotBeNull();
        result.Id.ShouldNotBe(Guid.Empty);
        result.Name.ShouldBe("test-tenant");
        result.StorageIdentifier.ShouldBeNull();
    }

    [Test]
    public void GetTenantContext_ShouldReturnContext_WhenOnlyStorageIdentifierExists()
    {
        // Arrange
        RequestContext.Set("TenantContext.Id", Guid.CreateVersion7().ToString());
        RequestContext.Set("TenantContext.StorageIdentifier", "storage-id-123");

        // Act
        var result = _provider.GetTenantContext();

        // Assert
        result.ShouldNotBeNull();
        result.Id.ShouldNotBe(Guid.Empty);
        result.Name.ShouldBeNull();
        result.StorageIdentifier.ShouldBe("storage-id-123");
    }
}
