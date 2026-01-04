using System.Diagnostics;
using System.Security.Claims;
using Dilcore.WebApi.Extensions;
using Microsoft.AspNetCore.Http;
using Shouldly;

namespace Dilcore.WebApi.Tests.Extensions;

[TestFixture]
public class UserTelemetryEnricherTests
{
    private DefaultHttpContext _httpContext;
    private UserTelemetryEnricher _enricher;

    [SetUp]
    public void Setup()
    {
        _httpContext = new DefaultHttpContext();
        _enricher = new UserTelemetryEnricher();
    }

    [Test]
    public void Enrich_ShouldSetUserIdTag_WhenUserIsAuthenticated()
    {
        // Arrange
        var activity = new Activity("TestActivity");
        var claims = new[] { new Claim(ClaimTypes.Name, "user-123") };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        _httpContext.User = new ClaimsPrincipal(identity);

        // Act
        _enricher.Enrich(activity, _httpContext.Request);

        // Assert
        activity.Tags.ShouldContain(t => t.Key == "user.id" && t.Value == "user-123");
    }

    [Test]
    public void Enrich_ShouldSetAnonymous_WhenUserIsNotAuthenticated()
    {
        // Arrange
        var activity = new Activity("TestActivity");
        // User identity is null/unauthenticated by default in DefaultHttpContext

        // Act
        _enricher.Enrich(activity, _httpContext.Request);

        // Assert
        activity.Tags.ShouldContain(t => t.Key == "user.id" && t.Value == "anonymous");
    }
}
