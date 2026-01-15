using Scalar.AspNetCore;
using Shouldly;

namespace Dilcore.Extensions.Scalar.Tests;

[TestFixture]
public class ScalarSettingsTests
{
    [Test]
    public void Constructor_ShouldInitializeWithDefaultValues()
    {
        // Act
        var settings = new ScalarSettings();

        // Assert
        settings.Title.ShouldBe("API Documentation");
        settings.Version.ShouldBe("v1");
        settings.Theme.ShouldBe(ScalarTheme.DeepSpace);
        settings.Endpoint.ShouldBe(ScalarConstants.Endpoint);
        settings.Authentication.ShouldBeNull();
    }

    [Test]
    public void AuthenticationSettings_ShouldInitializeWithDefaults()
    {
        // Act
        var settings = new ScalarAuthenticationSettings();

        // Assert
        settings.ClientId.ShouldBeNull();
        settings.ClientSecret.ShouldBeNull();
        settings.Audience.ShouldBeNull();
        settings.PreferredSecurityScheme.ShouldBeNull();
        settings.Scopes.ShouldBeEmpty();
    }
}
