using Dilcore.WebApp.Services.Tenancy;
using Shouldly;

namespace Dilcore.WebApp.Tests.Services.Tenancy;

[TestFixture]
public class BlazorTenantContextTests
{
    [Test]
    public void Set_AssignsSystemName()
    {
        IBlazorTenantContext context = new BlazorTenantContext();

        context.Set("acme");

        context.SystemName.ShouldBe("acme");
    }

    [Test]
    public void TwoInstances_AreIndependent()
    {
        IBlazorTenantContext a = new BlazorTenantContext();
        IBlazorTenantContext b = new BlazorTenantContext();

        a.Set("tenant-a");
        b.Set("tenant-b");

        a.SystemName.ShouldBe("tenant-a");
        b.SystemName.ShouldBe("tenant-b");
    }
}
