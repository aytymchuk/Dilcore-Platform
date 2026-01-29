using Dilcore.WebApp.Extensions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Shouldly;

namespace Dilcore.WebApp.Tests.Extensions;

public class DependencyInjectionTests
{
    [Test]
    public void AddWebAppServices_RegistersExpectedServices()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                {"AuthenticationSettings:Auth0:Domain", "test-domain"},
                {"AuthenticationSettings:Auth0:ClientId", "test-client-id"},
                {"AuthenticationSettings:Auth0:ClientSecret", "test-client-secret"},
                {"AuthenticationSettings:Auth0:Audience", "test-audience"},
                {"TelemetrySettings:ApplicationInsightsConnectionString", "InstrumentationKey=test-key;"},
                {"ApplicationSettings:Name", "TestApp"},
                {"ApiSettings:BaseUrl", "http://localhost"},
                {"ApiSettings:Retries", "3"}
            })
            .Build();
            
        var mockEnvironment = new Mock<IWebHostEnvironment>();
        mockEnvironment.Setup(e => e.EnvironmentName).Returns("Development");
        mockEnvironment.Setup(e => e.ApplicationName).Returns("TestApp");

        // Act
        services.AddLogging();
        services.AddWebAppServices(configuration, mockEnvironment.Object);

        // Register FakeNavigationManager to satisfy initialization requirements
        services.AddScoped<Microsoft.AspNetCore.Components.NavigationManager, FakeNavigationManager>();

        var provider = services.BuildServiceProvider(validateScopes: true);

        // Assert
        // Check for MudBlazor services
        // ISnackbar is a scoped service, so we must resolve it from a scope
        using var scope = provider.CreateScope();
        var snackbarProvider = scope.ServiceProvider.GetService<MudBlazor.ISnackbar>();
        snackbarProvider.ShouldNotBeNull();

        // Check for other expected services
        scope.ServiceProvider.GetService<Microsoft.AspNetCore.Components.Authorization.AuthenticationStateProvider>().ShouldNotBeNull();
        scope.ServiceProvider.GetService<Microsoft.AspNetCore.Authorization.IAuthorizationService>().ShouldNotBeNull();
        scope.ServiceProvider.GetService<Microsoft.AspNetCore.Components.NavigationManager>().ShouldNotBeNull();
    }

    private class FakeNavigationManager : Microsoft.AspNetCore.Components.NavigationManager
    {
        public FakeNavigationManager()
        {
            Initialize("http://localhost/", "http://localhost/");
        }
    }
}
