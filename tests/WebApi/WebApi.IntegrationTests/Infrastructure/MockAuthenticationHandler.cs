using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Dilcore.WebApi.IntegrationTests.Infrastructure;

public class MockAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    private readonly FakeUser _fakeUser;

    public MockAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        FakeUser fakeUser)
        : base(options, logger, encoder)
    {
        _fakeUser = fakeUser;
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (Request.Headers.ContainsKey("X-Test-Unauthorized") || !_fakeUser.IsAuthenticated)
        {
            return Task.FromResult(AuthenticateResult.Fail("Unauthorized via test header or FakeUser.IsAuthenticated = false"));
        }

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, _fakeUser.Name),
            new Claim(ClaimTypes.NameIdentifier, _fakeUser.UserId),
            new Claim(ClaimTypes.Email, _fakeUser.Email),
            new Claim("tenant.id", _fakeUser.TenantId)
        };

        var identity = new ClaimsIdentity(claims, "Test");
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, "Test");

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}