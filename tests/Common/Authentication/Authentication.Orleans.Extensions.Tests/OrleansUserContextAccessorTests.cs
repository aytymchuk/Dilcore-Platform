using Dilcore.Authentication.Abstractions;
using Shouldly;

namespace Dilcore.Authentication.Orleans.Extensions.Tests;

[TestFixture]
public class OrleansUserContextAccessorTests
{
    [TearDown]
    public void TearDown()
    {
        OrleansUserContextAccessor.SetUserContext(null);
    }

    [Test]
    public void SetUserContext_ShouldStoreContextInRequestContext()
    {
        var userContext = new UserContext("id", "email", "name", ["t1", "t2"], ["r1", "r2"]);

        OrleansUserContextAccessor.SetUserContext(userContext);

        var result = OrleansUserContextAccessor.GetUserContext();

        result.ShouldNotBeNull();
        result.Id.ShouldBe("id");
        result.Email.ShouldBe("email");
        result.FullName.ShouldBe("name");
        result.Tenants.ShouldBe(["t1", "t2"]);
        result.Roles.ShouldBe(["r1", "r2"]);
    }

    [Test]
    public void SetUserContext_ShouldClearContext_WhenNullPassed()
    {
        var userContext = new UserContext("id", "email", "name", [], []);
        OrleansUserContextAccessor.SetUserContext(userContext);

        OrleansUserContextAccessor.SetUserContext(null);

        var result = OrleansUserContextAccessor.GetUserContext();

        result.ShouldBeNull();
    }
}
