using Dilcore.Extensions.OpenApi.Abstractions;
using Dilcore.Extensions.Scalar;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Scalar.AspNetCore;
using Shouldly;

namespace Dilcore.Extensions.Scalar.Tests;

[TestFixture]
public class ScalarExtensionsTests
{
    [Test]
    public void UseScalarDocumentation_WithDefaultSettings_ShouldReturnApp()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();
        builder.Services.AddOpenApi();
        var app = builder.Build();

        // Act
        var result = app.UseScalarDocumentation();

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBe(app);
    }

    [Test]
    public void UseScalarDocumentation_WithCustomSettings_ShouldReturnApp()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();
        builder.Services.AddOpenApi();
        var app = builder.Build();

        const string customTitle = "My Custom API";
        const string customVersion = "v2.0";
        const string customEndpoint = "/custom-api-doc";

        // Act
        var result = app.UseScalarDocumentation(settings =>
        {
            settings.Title = customTitle;
            settings.Version = customVersion;
            settings.Endpoint = customEndpoint;
            settings.Theme = ScalarTheme.Mars;
        });

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBe(app);
    }

    [Test]
    public void UseScalarDocumentation_WithAuthentication_ShouldReturnApp()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();
        builder.Services.AddOpenApi();
        var app = builder.Build();

        const string clientId = "test-client-id";
        const string clientSecret = "test-client-secret";
        const string audience = "https://api.example.com";

        // Act
        var result = app.UseScalarDocumentation(settings =>
        {
            settings.Authentication = new ScalarAuthenticationSettings
            {
                ClientId = clientId,
                ClientSecret = clientSecret,
                Audience = audience,
                PreferredSecurityScheme = Constants.Security.Auth0SchemeName,
                Scopes = ["openid", "profile", "email"]
            };
        });

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBe(app);
    }

    [Test]
    public void UseScalarDocumentation_WithNullAuthentication_ShouldReturnApp()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();
        builder.Services.AddOpenApi();
        var app = builder.Build();

        // Act
        var result = app.UseScalarDocumentation(settings =>
        {
            settings.Authentication = null;
        });

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBe(app);
    }

    [Test]
    public void UseScalarDocumentation_WithConfigureAction_ShouldInvokeAction()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();
        builder.Services.AddOpenApi();
        var app = builder.Build();

        var configureInvoked = false;
        ScalarSettings? capturedSettings = null;

        // Act
        app.UseScalarDocumentation(settings =>
        {
            configureInvoked = true;
            capturedSettings = settings;
        });

        // Assert
        configureInvoked.ShouldBeTrue();
        capturedSettings.ShouldNotBeNull();
        capturedSettings.Title.ShouldBe("API Documentation");
        capturedSettings.Version.ShouldBe("v1");
        capturedSettings.Theme.ShouldBe(ScalarTheme.DeepSpace);
        capturedSettings.Endpoint.ShouldBe(ScalarConstants.Endpoint);
    }

    [Test]
    public void UseScalarDocumentation_WithoutConfigureAction_ShouldUseDefaults()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();
        builder.Services.AddOpenApi();
        var app = builder.Build();

        // Act  
        var result = app.UseScalarDocumentation();

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBe(app);
    }
}
