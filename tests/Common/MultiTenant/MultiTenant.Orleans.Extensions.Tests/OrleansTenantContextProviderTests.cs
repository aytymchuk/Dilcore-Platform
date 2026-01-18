using Dilcore.MultiTenant.Abstractions;
using Orleans.Runtime;
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
        RequestContext.Set("TenantContext.Name", "test-tenant");
        RequestContext.Set("TenantContext.StorageIdentifier", "storage-id-123");

        // Act
        var result = _provider.GetTenantContext();

        // Assert
        result.ShouldNotBeNull();
        result.Name.ShouldBe("test-tenant");
        result.StorageIdentifier.ShouldBe("storage-id-123");
    }

    [Test]
    public void GetTenantContext_ShouldReturnContext_WhenOnlyNameExists()
    {
        // Arrange
        RequestContext.Set("TenantContext.Name", "test-tenant");

        // Act
        var result = _provider.GetTenantContext();

        // Assert
        result.ShouldNotBeNull();
        result.Name.ShouldBe("test-tenant");
        result.StorageIdentifier.ShouldBeNull();
    }

    [Test]
    public void GetTenantContext_ShouldReturnContext_WhenOnlyStorageIdentifierExists()
    {
        // Arrange
        RequestContext.Set("TenantContext.StorageIdentifier", "storage-id-123");

        // Act
        var result = _provider.GetTenantContext();

        // Assert
        result.ShouldNotBeNull();
        result.Name.ShouldBeNull();
        result.StorageIdentifier.ShouldBe("storage-id-123");
    }

    [Test]
    public void Priority_ShouldBeHigherThanHttpProvider()
    {
        // The Orleans provider should have higher priority (200) than HTTP provider (100)
        // so it gets checked first in grain contexts

        // Act & Assert
        _provider.Priority.ShouldBeGreaterThan(100);
    }
}
