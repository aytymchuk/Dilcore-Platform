using Bunit;
using Bunit.TestDoubles;
using Dilcore.WebApp.Components.Layout;
using Dilcore.WebApp.Constants;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor;
using MudBlazor.Services;
using Shouldly;

using Microsoft.AspNetCore.Hosting;
using Moq;

namespace Dilcore.WebApp.Tests.Components.Layout;

public class LoginDisplayComponentTests : Bunit.TestContext
{
    private TestAuthorizationContext _authContext;

    public LoginDisplayComponentTests()
    {
        if (Services.GetService<ISnackbar>() == null)
        {
            Services.AddMudServices();
        }
        
        var mockEnv = new Mock<IWebHostEnvironment>();
        mockEnv.Setup(e => e.EnvironmentName).Returns("Development");
        Services.AddSingleton(mockEnv.Object);

        _authContext = this.AddTestAuthorization();
    }

    [Test]
    public void DisplaysUserName_WhenAuthenticated()
    {
        // Arrange
        _authContext.SetAuthorized("Test User");

        // Act
        var cut = RenderComponent<LoginDisplay>();

        // Assert
        cut.Markup.ShouldContain("Hello, Test User!");
        cut.Markup.ShouldContain("Sign Out");
        cut.Markup.ShouldNotContain("Sign In");
    }

    [Test]
    public void DisplaysSignInLink_WhenNotAuthenticated()
    {
        // Arrange
        _authContext.SetNotAuthorized();

        // Act
        var cut = RenderComponent<LoginDisplay>();

        // Assert
        cut.Markup.ShouldContain("Sign In");
        cut.Markup.ShouldNotContain("Hello,");
    }

    [Test]
    public void ShowsAccessTokenButton_WhenClaimExists()
    {
        // Arrange
        _authContext.SetAuthorized("Test User");
        _authContext.SetClaims(new System.Security.Claims.Claim(AuthConstants.AccessTokenClaim, "test-token"));

        // Act
        var cut = RenderComponent<LoginDisplay>();

        // Assert
        // Check for the key icon button which indicates the token functionality is present
        cut.FindAll("button").Any(b => b.ToMarkup().Contains("Key")).ShouldBeTrue();
    }

    [Test]
    public void DoesNotShowAccessTokenButton_WhenClaimMissing()
    {
        // Arrange
        _authContext.SetAuthorized("Test User");
        // No access_token claim

        // Act
        var cut = RenderComponent<LoginDisplay>();

        // Assert
        cut.FindAll("button").Any(b => b.ToMarkup().Contains("Key")).ShouldBeFalse();
    }
}
