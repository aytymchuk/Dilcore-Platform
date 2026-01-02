using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Dilcore.WebApi.IntegrationTests.Infrastructure;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    public FakeUser FakeUser { get; } = new FakeUser();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((context, config) =>
        {
            // Don't clear sources - just add the Testing config to override
            config.AddJsonFile("appsettings.Testing.json", optional: false, reloadOnChange: false);
        });

        builder.ConfigureServices(services =>
        {
            // Register FakeUser as a singleton so it can be accessed and modified during tests
            services.AddSingleton(FakeUser);

            // Remove services that cause side effects or conflicts in tests
            RemoveOpenTelemetryServices(services);
            RemoveAuthenticationServices(services);

            // Register test authentication
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = "Test";
                options.DefaultChallengeScheme = "Test";
            })
            .AddScheme<AuthenticationSchemeOptions, MockAuthenticationHandler>("Test", options => { });
        });
    }

    /// <summary>
    /// Configure the fake user for testing. This can be called before or during test execution.
    /// </summary>
    public CustomWebApplicationFactory ConfigureFakeUser(Action<FakeUser> configure)
    {
        configure(FakeUser);
        return this;
    }

    private static void RemoveOpenTelemetryServices(IServiceCollection services)
    {
        var otelServices = services.Where(d =>
            (d.ServiceType.Namespace?.StartsWith("OpenTelemetry") == true) ||
            (d.ImplementationType?.Namespace?.StartsWith("OpenTelemetry") == true) ||
            (d.ServiceType.IsGenericType && d.ServiceType.GetGenericArguments().Any(t => t.Namespace?.StartsWith("OpenTelemetry") == true)) ||
            (d.ImplementationFactory?.Method.DeclaringType?.Namespace?.StartsWith("OpenTelemetry") == true) ||
            (d.ImplementationFactory?.Method.Module.Assembly.FullName?.Contains("OpenTelemetry") == true)
        ).ToList();

        foreach (var descriptor in otelServices)
        {
            services.Remove(descriptor);
        }
    }

    private static void RemoveAuthenticationServices(IServiceCollection services)
    {
        var authServices = services.Where(d =>
            (d.ServiceType.FullName?.Contains("Authentication", StringComparison.OrdinalIgnoreCase) == true) ||
            (d.ServiceType.FullName?.Contains("JwtBearer", StringComparison.OrdinalIgnoreCase) == true) ||
            (d.ServiceType.FullName?.Contains("Auth0", StringComparison.OrdinalIgnoreCase) == true)
        ).ToList();

        foreach (var descriptor in authServices)
        {
            services.Remove(descriptor);
        }
    }
}

