using Shouldly;

namespace Dilcore.MultiTenant.Abstractions.Tests;

[TestFixture]
public class TenantContextTests
{
    [Test]
    public void TenantContext_IsImmutable()
    {
        // Arrange & Act
        var id = Guid.CreateVersion7();
        var context = new TenantContext(id, "Tenant1", "storage-01");

        // Assert
        context.Id.ShouldBe(id);
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
        context.Id.ShouldBe(Guid.Empty);
        context.Name.ShouldBeNull();
        context.StorageIdentifier.ShouldBeNull();
    }

    [Test]
    public void TenantContext_Equality_WorksByValue()
    {
        // Arrange
        var id1 = Guid.CreateVersion7();
        var id2 = Guid.CreateVersion7();
        var context1 = new TenantContext(id1, "T1", "storage");
        var context2 = new TenantContext(id1, "T1", "storage");
        var context3 = new TenantContext(id2, "T1", "storage");

        // Assert - Records have value equality
        context1.ShouldBe(context2);
        context1.ShouldNotBe(context3);
    }
}