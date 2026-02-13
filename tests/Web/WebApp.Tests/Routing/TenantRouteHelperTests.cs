using Dilcore.WebApp.Routing;
using Shouldly;

namespace Dilcore.WebApp.Tests.Routing;

public class TenantRouteHelperTests
{

    [TestCase("/workspaces/acme", ExpectedResult = "acme")]
    [TestCase("/workspaces/acme/settings", ExpectedResult = "acme")]
    [TestCase("/workspaces/acme/settings/billing", ExpectedResult = "acme")]
    [TestCase("/workspaces/my-tenant", ExpectedResult = "my-tenant")]
    public string? ExtractTenantFromPath_WithTenantRoute_ReturnsTenantName(string path)
    {
        return TenantRouteHelper.ExtractTenantFromPath(path);
    }

    [TestCase(null)]
    [TestCase("")]
    [TestCase("/")]
    [TestCase("/register")]
    [TestCase("/account/login")]
    [TestCase("/counter")]
    [TestCase("/weather")]
    [TestCase("/error")]
    [TestCase("/_blazor/negotiate")]
    [TestCase("/workspaces")]
    public void ExtractTenantFromPath_WithNonTenantRoute_ReturnsNull(string? path)
    {
        var result = TenantRouteHelper.ExtractTenantFromPath(path);

        result.ShouldBeNull();
    }

    [Test]
    public void ExtractTenantFromPath_IsCaseInsensitiveForPrefix()
    {
        var result = TenantRouteHelper.ExtractTenantFromPath("/WORKSPACES/acme");

        result.ShouldBe("acme");
    }

    [Test]
    public void ExtractTenantFromPath_PreservesTenantCasing()
    {
        var result = TenantRouteHelper.ExtractTenantFromPath("/workspaces/MyTenant");

        result.ShouldBe("MyTenant");
    }
}
