using Dilcore.MultiTenant.Http.Extensions;
using Dilcore.WebApp.Extensions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
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
                {"ApplicationSettings:Name", "TestApp"}
            })
            .Build();
            
        var mockEnvironment = new Mock<IWebHostEnvironment>();
        mockEnvironment.Setup(e => e.EnvironmentName).Returns("Development");

        // Act
        services.AddMultiTenancy();
        services.AddWebAppServices(configuration, mockEnvironment.Object);
        var provider = services.BuildServiceProvider();

        // Assert
        // Check for MudBlazor services
        var snackbarProvider = provider.GetService<MudBlazor.ISnackbar>();
        snackbarProvider.ShouldNotBeNull();
    }
}
