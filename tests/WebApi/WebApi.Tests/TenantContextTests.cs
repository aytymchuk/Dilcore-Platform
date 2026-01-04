using Dilcore.WebApi.Infrastructure.MultiTenant;
using Shouldly;

namespace Dilcore.WebApi.Tests;

[TestFixture]
public class TenantContextTests
{
    [Test]
    public void TenantContext_IsImmutable()
    {
        // Arrange & Act
        var context = new TenantContext("Tenant1", "storage-01");

        // Assert
        context.Name.ShouldBe("Tenant1");
        context.StorageIdentifier.ShouldBe("storage-01");

        // Cannot modify - it's a record (compile-time enforcement)
    }

    [Test]
    public void TenantContext_Empty_HasNullValues()
    {
        // Arrange & Act
        var context = TenantContext.Empty;

        // Assert
        context.Name.ShouldBeNull();
        context.StorageIdentifier.ShouldBeNull();
    }

    [Test]
    public void TenantContext_Equality_WorksByValue()
    {
        // Arrange
        var context1 = new TenantContext("T1", "storage");
        var context2 = new TenantContext("T1", "storage");
        var context3 = new TenantContext("T2", "storage");

        // Assert - Records have value equality
        context1.ShouldBe(context2);
        context1.ShouldNotBe(context3);
    }
}