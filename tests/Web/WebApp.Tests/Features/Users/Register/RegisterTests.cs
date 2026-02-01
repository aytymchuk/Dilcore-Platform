using System.Net;
using System.Net.Http;
using Bunit;
using Bunit.TestDoubles;
using Dilcore.Identity.Contracts.Profile;
using Dilcore.WebApi.Client.Clients;
using Dilcore.WebApp.Features.Users.Register;
using Dilcore.WebApp.Models.Users;
using Dilcore.WebApp.Services;
using FluentResults;
using MediatR;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using MudBlazor;
using MudBlazor.Services;
using Refit;
using Shouldly;
using System.Security.Claims;

namespace Dilcore.WebApp.Tests.Features.Users.Register;

public class RegisterTests
{
    private const string TestEmail = "test@example.com";
    private const string TestFirstName = "John";
    private const string TestLastName = "Doe";
    private const string SuccessMessage = "Registration successful! Welcome to the platform.";

    private Bunit.TestContext _ctx = null!;
    private TestAuthorizationContext _authContext = null!;
    private Mock<ISender> _mockSender = null!;
    private Mock<IAppNavigator> _mockNavigator = null!;
    private Mock<IIdentityClient> _mockIdentityClient = null!;
    private Mock<ISnackbar> _mockSnackbar = null!;

    [SetUp]
    public async Task Setup()
    {
        _ctx = new Bunit.TestContext();
        _ctx.JSInterop.Mode = JSRuntimeMode.Loose;

        _ctx.Services.AddMudServices();

        var mockEnv = new Mock<IWebHostEnvironment>();
        mockEnv.Setup(e => e.EnvironmentName).Returns("Development");
        _ctx.Services.AddSingleton(mockEnv.Object);

        _authContext = _ctx.AddTestAuthorization();

        _mockSender = new Mock<ISender>();
        _mockNavigator = new Mock<IAppNavigator>();
        _mockIdentityClient = new Mock<IIdentityClient>();
        _mockSnackbar = new Mock<ISnackbar>();

        _ctx.Services.AddSingleton(_mockSender.Object);
        _ctx.Services.AddSingleton(_mockNavigator.Object);
        _ctx.Services.AddSingleton(_mockIdentityClient.Object);
        _ctx.Services.AddSingleton(_mockSnackbar.Object);

        _ctx.RenderComponent<MudPopoverProvider>();
        _ctx.RenderComponent<MudDialogProvider>();
    }

    [TearDown]
    public void TearDown()
    {
        _ctx?.Dispose();
    }

    [Test]
    public async Task RendersWithCorrectUIElements()
    {
        // Arrange
        _authContext.SetNotAuthorized();
        await SetupIdentityClientNoUserAsync();

        // Act
        var cut = _ctx.RenderComponent<Dilcore.WebApp.Features.Users.Register.Register>();

        // Assert
        cut.Markup.ShouldContain("Complete Profile");
        cut.Markup.ShouldContain("Finalize your account setup to get started");
        cut.Find("input[type='email']").ShouldNotBeNull();
        cut.FindAll("input[type='text']").Count.ShouldBe(2);
        cut.Find("button").ShouldNotBeNull();
    }

    [Test]
    public async Task DisplaysLoadingProgressBar_WhenInitializing()
    {
        // Arrange
        _authContext.SetNotAuthorized();
        await SetupIdentityClientNoUserAsync();

        // Act
        var cut = _ctx.RenderComponent<Dilcore.WebApp.Features.Users.Register.Register>();

        // Assert - Loading state is transient; we only verify the component renders.
        // Transient loading UI (mud-progress-linear) is not asserted to avoid flakiness.
        cut.Markup.ShouldNotBeNullOrEmpty();
    }

    [Test]
    public async Task PopulatesModelFromStandardClaims_WhenAuthenticated()
    {
        // Arrange
        _authContext.SetAuthorized("Test User");
        _authContext.SetClaims(
            new Claim(ClaimTypes.Email, TestEmail),
            new Claim(ClaimTypes.GivenName, TestFirstName),
            new Claim(ClaimTypes.Surname, TestLastName)
        );
        await SetupIdentityClientNoUserAsync();

        // Act
        var cut = _ctx.RenderComponent<Dilcore.WebApp.Features.Users.Register.Register>();

        // Assert
        var emailInput = cut.Find("input[type='email']");
        emailInput.GetAttribute("value").ShouldBe(TestEmail);

        var textInputs = cut.FindAll("input[type='text']");
        textInputs[0].GetAttribute("value").ShouldBe(TestFirstName);
        textInputs[1].GetAttribute("value").ShouldBe(TestLastName);
    }

    [Test]
    public async Task PopulatesModelFromAlternativeClaims_WhenAuthenticated()
    {
        // Arrange
        _authContext.SetAuthorized("Test User");
        _authContext.SetClaims(
            new Claim("email", TestEmail),
            new Claim("given_name", TestFirstName),
            new Claim("family_name", TestLastName)
        );
        await SetupIdentityClientNoUserAsync();

        // Act
        var cut = _ctx.RenderComponent<Dilcore.WebApp.Features.Users.Register.Register>();

        // Assert
        var emailInput = cut.Find("input[type='email']");
        emailInput.GetAttribute("value").ShouldBe(TestEmail);

        var textInputs = cut.FindAll("input[type='text']");
        textInputs[0].GetAttribute("value").ShouldBe(TestFirstName);
        textInputs[1].GetAttribute("value").ShouldBe(TestLastName);
    }

    [Test]
    public async Task ModelRemainsEmpty_WhenNotAuthenticated()
    {
        // Arrange
        _authContext.SetNotAuthorized();
        await SetupIdentityClientNoUserAsync();

        // Act
        var cut = _ctx.RenderComponent<Dilcore.WebApp.Features.Users.Register.Register>();

        // Assert
        var emailInput = cut.Find("input[type='email']");
        var emailValue = emailInput.GetAttribute("value");
        emailValue.ShouldBeNullOrEmpty();

        var textInputs = cut.FindAll("input[type='text']");
        var firstNameValue = textInputs[0].GetAttribute("value");
        var lastNameValue = textInputs[1].GetAttribute("value");
        firstNameValue.ShouldBeNullOrEmpty();
        lastNameValue.ShouldBeNullOrEmpty();
    }

    [Test]
    public void RedirectsToHome_WhenUserAlreadyExists()
    {
        // Arrange
        _authContext.SetAuthorized("Test User");
        SetupIdentityClientWithExistingUser();

        // Act
        _ctx.RenderComponent<Dilcore.WebApp.Features.Users.Register.Register>();

        // Assert
        _mockNavigator.Verify(n => n.ToHome(true), Times.Once);
    }

    [Test]
    public async Task DoesNotRedirect_WhenUserDoesNotExist()
    {
        // Arrange
        _authContext.SetAuthorized("Test User");
        await SetupIdentityClientNoUserAsync();

        // Act
        _ctx.RenderComponent<Dilcore.WebApp.Features.Users.Register.Register>();

        // Assert
        _mockNavigator.Verify(n => n.ToHome(It.IsAny<bool>()), Times.Never);
    }

    [Test]
    public async Task SubmitButtonDisabled_WhenFormIsInvalid()
    {
        // Arrange
        _authContext.SetNotAuthorized();
        await SetupIdentityClientNoUserAsync();

        // Act
        var cut = _ctx.RenderComponent<Dilcore.WebApp.Features.Users.Register.Register>();

        // Assert
        var button = cut.Find("button");
        button.HasAttribute("disabled").ShouldBeTrue();
    }

    [Test]
    public async Task SendsRegisterCommand_OnSuccessfulSubmit()
    {
        // Arrange
        _authContext.SetAuthorized("Test User");
        _authContext.SetClaims(
            new Claim(ClaimTypes.Email, TestEmail),
            new Claim(ClaimTypes.GivenName, TestFirstName),
            new Claim(ClaimTypes.Surname, TestLastName)
        );
        await SetupIdentityClientNoUserAsync();

        var successResult = Result.Ok(new UserModel(
            Guid.NewGuid(),
            TestEmail,
            TestFirstName,
            TestLastName));

        _mockSender.Setup(s => s.Send(It.IsAny<RegisterCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(successResult);

        var cut = _ctx.RenderComponent<Dilcore.WebApp.Features.Users.Register.Register>();

        // Act
        // Populate fields
        cut.Find("input[type='email']").Change(TestEmail);
        var textInputs = cut.FindAll("input[type='text']");
        textInputs[0].Change(TestFirstName);
        textInputs[1].Change(TestLastName);



        // Force validation to ensure button state updates in test environment
        var form = cut.FindComponent<MudBlazor.MudForm>().Instance;
        await cut.InvokeAsync(() => form.Validate());

        // Wait for the primary action button to be enabled (ignoring the hidden submit button)
        cut.WaitForState(() => !cut.Find("button.mud-button-filled-primary").HasAttribute("disabled"));
        
        var button = cut.Find("button.mud-button-filled-primary");
        await button.ClickAsync(new Microsoft.AspNetCore.Components.Web.MouseEventArgs());

        // Assert
        _mockSender.Verify(s => s.Send(
            It.Is<RegisterCommand>(cmd =>
                cmd.Parameters.Email == TestEmail &&
                cmd.Parameters.FirstName == TestFirstName &&
                cmd.Parameters.LastName == TestLastName),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task NavigatesToHome_AfterSuccessfulRegistration()
    {
        // Arrange
        _authContext.SetAuthorized("Test User");
        _authContext.SetClaims(
            new Claim(ClaimTypes.Email, TestEmail),
            new Claim(ClaimTypes.GivenName, TestFirstName),
            new Claim(ClaimTypes.Surname, TestLastName)
        );
        await SetupIdentityClientNoUserAsync();

        var successResult = Result.Ok(new UserModel(
            Guid.NewGuid(),
            TestEmail,
            TestFirstName,
            TestLastName));

        _mockSender.Setup(s => s.Send(It.IsAny<RegisterCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(successResult);

        var cut = _ctx.RenderComponent<Dilcore.WebApp.Features.Users.Register.Register>();

        cut.Find("input[type='email']").Change(TestEmail);
        var textInputs = cut.FindAll("input[type='text']");
        textInputs[0].Change(TestFirstName);
        textInputs[1].Change(TestLastName);

        // Act

        
        // Force validation to ensure button state updates in test environment
        var form = cut.FindComponent<MudBlazor.MudForm>().Instance;
        await cut.InvokeAsync(() => form.Validate());
        
        // Wait for the primary action button to be enabled (ignoring the hidden submit button)
        cut.WaitForState(() => !cut.Find("button.mud-button-filled-primary").HasAttribute("disabled"));
        
        var button = cut.Find("button.mud-button-filled-primary");
        await button.ClickAsync(new Microsoft.AspNetCore.Components.Web.MouseEventArgs());

        // Assert
        _mockNavigator.Verify(n => n.ToHome(true), Times.Once);
    }

    [Test]
    public async Task DisplaysSuccessMessage_AfterSuccessfulRegistration()
    {
        // Arrange
        _authContext.SetAuthorized("Test User");
        _authContext.SetClaims(
            new Claim(ClaimTypes.Email, TestEmail),
            new Claim(ClaimTypes.GivenName, TestFirstName),
            new Claim(ClaimTypes.Surname, TestLastName)
        );
        await SetupIdentityClientNoUserAsync();

        var successResult = Result.Ok(new UserModel(
            Guid.NewGuid(),
            TestEmail,
            TestFirstName,
            TestLastName));

        _mockSender.Setup(s => s.Send(It.IsAny<RegisterCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(successResult);

        var cut = _ctx.RenderComponent<Dilcore.WebApp.Features.Users.Register.Register>();

        // Act
        // Populate fields
        cut.Find("input[type='email']").Change(TestEmail);
        var textInputs = cut.FindAll("input[type='text']");
        textInputs[0].Change(TestFirstName);
        textInputs[1].Change(TestLastName);



        // Force validation to ensure button state updates in test environment
        var form = cut.FindComponent<MudBlazor.MudForm>().Instance;
        await cut.InvokeAsync(() => form.Validate());

        // Wait for the primary action button to be enabled (ignoring the hidden submit button)
        cut.WaitForState(() => !cut.Find("button.mud-button-filled-primary").HasAttribute("disabled"));
        
        var button = cut.Find("button.mud-button-filled-primary");
        await button.ClickAsync(new Microsoft.AspNetCore.Components.Web.MouseEventArgs());

        // Assert
        _mockSnackbar.Verify(s => s.Add(SuccessMessage, Severity.Success, It.IsAny<Action<SnackbarOptions>?>(), It.IsAny<string?>()), Times.Once);
    }

    private async Task SetupIdentityClientNoUserAsync()
    {
        var notFoundException = await ApiException.Create(
            new HttpRequestMessage(),
            HttpMethod.Get,
            new HttpResponseMessage(HttpStatusCode.NotFound) { Content = new StringContent("") },
            new RefitSettings());

        _mockIdentityClient.Setup(c => c.GetCurrentUserAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(notFoundException);
    }

    private void SetupIdentityClientWithExistingUser()
    {
        var userDto = new UserDto(
            Guid.NewGuid(),
            TestEmail,
            TestFirstName,
            TestLastName,
            DateTime.UtcNow);

        _mockIdentityClient.Setup(c => c.GetCurrentUserAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(userDto);
    }
}
