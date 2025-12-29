using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;

namespace WebApi.IntegrationTests;

public class MockAuthenticationOptions : AuthenticationSchemeOptions
{
    public List<Claim> Claims { get; set; } = new()
    {
        new Claim(ClaimTypes.Name, "TestUser"),
        new Claim(ClaimTypes.NameIdentifier, "test-user-id"),
        new Claim("tenant.id", "test-tenant")
    };
}