using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace WebApi.IntegrationTests;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    public CustomWebApplicationFactory()
    {
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Testing");
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((context, config) =>
        {
            config.Sources.Clear();
            config.AddJsonFile("appsettings.Testing.json");
        });

        builder.ConfigureServices(services =>
        {
            // Remove OpenTelemetry services to prevent external connections/side effects during tests
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

            // Remove existing Authentication setup (Auth0, JwtBearer, etc.) to start fresh for testing
            var authServices = services.Where(d =>
                (d.ServiceType.FullName?.Contains("Authentication", StringComparison.OrdinalIgnoreCase) == true) ||
                (d.ServiceType.FullName?.Contains("JwtBearer", StringComparison.OrdinalIgnoreCase) == true) ||
                (d.ServiceType.FullName?.Contains("Auth0", StringComparison.OrdinalIgnoreCase) == true)
            ).ToList();

            foreach (var descriptor in authServices)
            {
                services.Remove(descriptor);
            }

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = "Test";
                options.DefaultChallengeScheme = "Test";
            })
            .AddScheme<AuthenticationSchemeOptions, MockAuthenticationHandler>("Test", options => { });
        });
    }
}
