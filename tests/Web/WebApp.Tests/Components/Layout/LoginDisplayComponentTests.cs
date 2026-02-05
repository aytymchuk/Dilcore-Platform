using Bunit;
using Bunit.TestDoubles;
using Dilcore.WebApp.Components.Layout;
using Dilcore.WebApp.Constants;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using MudBlazor;
using MudBlazor.Services;
using Shouldly;

namespace Dilcore.WebApp.Tests.Components.Layout;

public class LoginDisplayComponentTests
{
    private Bunit.TestContext _ctx;
    private TestAuthorizationContext _authContext;

    [SetUp]
    public void Setup()
    {
        _ctx = new Bunit.TestContext();
        _ctx.JSInterop.Mode = JSRuntimeMode.Loose;

        _ctx.Services.AddMudServices();

        var mockEnv = new Mock<IWebHostEnvironment>();
        mockEnv.Setup(e => e.EnvironmentName).Returns("Development");
        _ctx.Services.AddSingleton(mockEnv.Object);

        _authContext = _ctx.AddTestAuthorization();

        _ctx.RenderComponent<MudPopoverProvider>();
    }

    [TearDown]
    public void TearDown()
    {
        _ctx?.Dispose();
    }

    [Test]
    public void DisplaysUserName_WhenAuthenticated()
    {
        // Arrange
        _authContext.SetAuthorized("Test User");

        // Act
        var cut = _ctx.RenderComponent<LoginDisplay>();

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
        var cut = _ctx.RenderComponent<LoginDisplay>();

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
        var cut = _ctx.RenderComponent<LoginDisplay>();

        // Assert
        // Check for the key icon button which indicates the token functionality is present
        cut.Find("[data-testid='access-token-button']").ShouldNotBeNull();
    }

    [Test]
    public void DoesNotShowAccessTokenButton_WhenClaimMissing()
    {
        // Arrange
        _authContext.SetAuthorized("Test User");
        // No access_token claim

        // Act
        var cut = _ctx.RenderComponent<LoginDisplay>();

        // Assert
        cut.FindAll("[data-testid='access-token-button']").Count.ShouldBe(0);
    }
}
