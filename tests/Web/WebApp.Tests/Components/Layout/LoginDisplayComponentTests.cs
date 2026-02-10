using Bunit;
using Bunit.TestDoubles;
using Dilcore.WebApp.Components.Layout;
using Dilcore.WebApp.Features.Users;
using Dilcore.WebApp.Models.Users;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using MudBlazor;
using MudBlazor.Services;
using Shouldly;

namespace Dilcore.WebApp.Tests.Components.Layout;

public class LoginDisplayComponentTests
{
    private Bunit.TestContext _ctx = default!;
    private TestAuthorizationContext _authContext = default!;

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
    public void DisplaysAvatar_WhenAuthenticated_WithUserState()
    {
        // Arrange
        _authContext.SetAuthorized("Test User");
        var userState = CreateUserStateProvider(
            new UserModel(Guid.NewGuid(), "test@example.com", "Test", "User"));

        // Act
        var cut = RenderWithCascadingUserState(userState);

        // Assert — MudAvatar is rendered for authenticated users
        cut.FindAll(".mud-avatar").Count.ShouldBeGreaterThan(0);
        cut.Markup.ShouldNotContain("Sign In");
    }

    [Test]
    public void DisplaysAvatarWithFallbackInitial_WhenAuthenticated_WithoutCurrentUser()
    {
        // Arrange
        _authContext.SetAuthorized("Test User");
        var userState = CreateUserStateProvider(currentUser: null);

        // Act
        var cut = RenderWithCascadingUserState(userState);

        // Assert — avatar renders with fallback, no Sign In button
        cut.FindAll(".mud-avatar").Count.ShouldBeGreaterThan(0);
        cut.Markup.ShouldNotContain("Sign In");
    }

    [Test]
    public void DisplaysSignInButton_WhenNotAuthenticated()
    {
        // Arrange
        _authContext.SetNotAuthorized();

        // Act
        var cut = RenderWithCascadingUserState(CreateUserStateProvider(currentUser: null));

        // Assert
        cut.Markup.ShouldContain("Sign In");
        cut.FindAll(".mud-avatar").Count.ShouldBe(0);
    }

    [Test]
    public void RendersMenuComponent_WhenAuthenticated()
    {
        // Arrange
        _authContext.SetAuthorized("Test User");
        var userState = CreateUserStateProvider(
            new UserModel(Guid.NewGuid(), "test@example.com", "Test", "User"));

        // Act
        var cut = RenderWithCascadingUserState(userState);

        // Assert — MudMenu wrapper is rendered (menu items are in the popover)
        cut.FindAll(".mud-menu").Count.ShouldBe(1);
    }

    private IRenderedComponent<LoginDisplay> RenderWithCascadingUserState(UserStateProvider userState)
    {
        return _ctx.RenderComponent<LoginDisplay>(parameters =>
            parameters.Add(p => p.UserState, userState));
    }

    private static UserStateProvider CreateUserStateProvider(UserModel? currentUser)
    {
        var provider = new UserStateProvider();

        if (currentUser is null)
        {
            return provider;
        }

        var property = typeof(UserStateProvider).GetProperty(nameof(UserStateProvider.CurrentUser));
        property!.SetValue(provider, currentUser);

        return provider;
    }
}
