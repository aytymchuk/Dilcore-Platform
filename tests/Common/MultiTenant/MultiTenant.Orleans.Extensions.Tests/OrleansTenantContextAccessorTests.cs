using Dilcore.MultiTenant.Abstractions;
using Shouldly;

namespace Dilcore.MultiTenant.Orleans.Extensions.Tests;

/// <summary>
/// Unit tests for OrleansTenantContextAccessor.
/// </summary>
[TestFixture]
public class OrleansTenantContextAccessorTests
{
    [SetUp]
    public void SetUp()
    {
        // Clear RequestContext before each test
        RequestContext.Clear();
    }

    [Test]
    public void SetTenantContext_ShouldStoreContextInRequestContext()
    {
        // Arrange
        var tenantContext = new TenantContext(Guid.NewGuid(), "test-tenant", "storage-id-123");

        // Act
        OrleansTenantContextAccessor.SetTenantContext(tenantContext);

        // Assert
        var id = RequestContext.Get("TenantContext.Id");
        var name = RequestContext.Get("TenantContext.Name");
        var storageId = RequestContext.Get("TenantContext.StorageIdentifier");

        id.ShouldBe(tenantContext.Id);
        name.ShouldBe("test-tenant");
        storageId.ShouldBe("storage-id-123");
    }

    [Test]
    public void GetTenantContext_ShouldRetrieveContextFromRequestContext()
    {
        // Arrange
        RequestContext.Set("TenantContext.Id", Guid.NewGuid().ToString());
        RequestContext.Set("TenantContext.Name", "test-tenant");
        RequestContext.Set("TenantContext.StorageIdentifier", "storage-id-123");

        // Act
        var result = OrleansTenantContextAccessor.GetTenantContext();

        // Assert
        result.ShouldNotBeNull();
        result.Id.ShouldNotBe(Guid.Empty);
        result.Name.ShouldBe("test-tenant");
        result.StorageIdentifier.ShouldBe("storage-id-123");
    }

    [Test]
    public void GetTenantContext_ShouldReturnNull_WhenNoContextSet()
    {
        // Act
        var result = OrleansTenantContextAccessor.GetTenantContext();

        // Assert
        result.ShouldBeNull();
    }

    [Test]
    public void SetTenantContext_WithNull_ShouldRemoveContextFromRequestContext()
    {
        // Arrange
        RequestContext.Set("TenantContext.Id", Guid.NewGuid().ToString());
        RequestContext.Set("TenantContext.Name", "test-tenant");
        RequestContext.Set("TenantContext.StorageIdentifier", "storage-id-123");

        // Act
        OrleansTenantContextAccessor.SetTenantContext(null);

        // Assert
        var result = OrleansTenantContextAccessor.GetTenantContext();
        result.ShouldBeNull();
    }

    [Test]
    public void SetTenantContext_WithNullName_ShouldRemoveNameFromRequestContext()
    {
        // Arrange
        var tenantContext = new TenantContext(Guid.NewGuid(), null, "storage-id-123");

        // Act
        OrleansTenantContextAccessor.SetTenantContext(tenantContext);

        // Assert
        var name = RequestContext.Get("TenantContext.Name");
        var storageId = RequestContext.Get("TenantContext.StorageIdentifier");

        name.ShouldBeNull();
        storageId.ShouldBe("storage-id-123");
    }

    [Test]
    public void SetTenantContext_WithNullStorageIdentifier_ShouldRemoveStorageIdentifierFromRequestContext()
    {
        // Arrange
        var tenantContext = new TenantContext(Guid.NewGuid(), "test-tenant", null);

        // Act
        OrleansTenantContextAccessor.SetTenantContext(tenantContext);

        // Assert
        var name = RequestContext.Get("TenantContext.Name");
        var storageId = RequestContext.Get("TenantContext.StorageIdentifier");

        name.ShouldBe("test-tenant");
        storageId.ShouldBeNull();
    }

    [Test]
    public void GetTenantContext_ShouldReturnContextWithOnlyName_WhenOnlyNameSet()
    {
        // Arrange
        RequestContext.Set("TenantContext.Id", Guid.NewGuid().ToString());
        RequestContext.Set("TenantContext.Name", "test-tenant");

        // Act
        var result = OrleansTenantContextAccessor.GetTenantContext();

        // Assert
        result.ShouldNotBeNull();
        result.Name.ShouldBe("test-tenant");
        result.StorageIdentifier.ShouldBeNull();
    }

    [Test]
    public void GetTenantContext_ShouldReturnContextWithOnlyStorageIdentifier_WhenOnlyStorageIdentifierSet()
    {
        // Arrange
        RequestContext.Set("TenantContext.Id", Guid.NewGuid().ToString());
        RequestContext.Set("TenantContext.StorageIdentifier", "storage-id-123");

        // Act
        var result = OrleansTenantContextAccessor.GetTenantContext();

        // Assert
        result.ShouldNotBeNull();
        result.Name.ShouldBeNull();
        result.StorageIdentifier.ShouldBe("storage-id-123");
    }

    [Test]
    public void SetThenGetTenantContext_ShouldRoundTrip()
    {
        // Arrange
        var original = new TenantContext(Guid.NewGuid(), "my-tenant", "my-storage-123");

        // Act
        OrleansTenantContextAccessor.SetTenantContext(original);
        var retrieved = OrleansTenantContextAccessor.GetTenantContext();

        // Assert
        retrieved.ShouldNotBeNull();
        retrieved.Id.ShouldBe(original.Id);
        retrieved.Name.ShouldBe(original.Name);
        retrieved.StorageIdentifier.ShouldBe(original.StorageIdentifier);
    }
}
