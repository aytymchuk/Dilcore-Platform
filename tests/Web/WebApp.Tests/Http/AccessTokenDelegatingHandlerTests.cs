using System.Net;
using System.Security.Claims;
using Dilcore.MultiTenant.Abstractions;
using Dilcore.WebApp.Constants;
using Dilcore.WebApp.Http;
using Dilcore.WebApp.Services.Tenancy;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Shouldly;

namespace Dilcore.WebApp.Tests.Http;

[TestFixture]
public class AccessTokenDelegatingHandlerTests
{
    [Test]
    public async Task SendAsync_AddsXTenant_WhenCircuitHasTenantContext_AndUserIsAuthenticated()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IBlazorTenantContext>(_ =>
        {
            var ctx = new BlazorTenantContext();
            ctx.Set("acme");
            return ctx;
        });
        await using var provider = services.BuildServiceProvider();

        var circuitAccessor = new CircuitServicesAccessor { Services = provider };
        var capturing = new CapturingHandler();
        var handler = CreateHandler(circuitAccessor, capturing);

        using var invoker = new HttpMessageInvoker(handler);
        var request = new HttpRequestMessage(HttpMethod.Get, "https://api.example.com/");
        await invoker.SendAsync(request, CancellationToken.None);

        capturing.LastRequest.ShouldNotBeNull();
        capturing.LastRequest!.Headers.TryGetValues(TenantConstants.HeaderName, out var values).ShouldBeTrue();
        values!.Single().ShouldBe("acme");
    }

    [Test]
    public async Task SendAsync_DoesNotAddXTenant_WhenCircuitServicesAccessorHasNoScope()
    {
        var circuitAccessor = new CircuitServicesAccessor { Services = null };
        var capturing = new CapturingHandler();
        var handler = CreateHandler(circuitAccessor, capturing);

        using var invoker = new HttpMessageInvoker(handler);
        var request = new HttpRequestMessage(HttpMethod.Get, "https://api.example.com/");
        await invoker.SendAsync(request, CancellationToken.None);

        capturing.LastRequest.ShouldNotBeNull();
        capturing.LastRequest!.Headers.Contains(TenantConstants.HeaderName).ShouldBeFalse();
    }

    [Test]
    public async Task SendAsync_DoesNotAddXTenant_WhenTenantContextIsUnset()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IBlazorTenantContext, BlazorTenantContext>();
        await using var provider = services.BuildServiceProvider();

        var circuitAccessor = new CircuitServicesAccessor { Services = provider };
        var capturing = new CapturingHandler();
        var handler = CreateHandler(circuitAccessor, capturing);

        using var invoker = new HttpMessageInvoker(handler);
        var request = new HttpRequestMessage(HttpMethod.Get, "https://api.example.com/");
        await invoker.SendAsync(request, CancellationToken.None);

        capturing.LastRequest.ShouldNotBeNull();
        capturing.LastRequest!.Headers.Contains(TenantConstants.HeaderName).ShouldBeFalse();
    }

    [Test]
    public async Task SendAsync_DoesNotOverwriteExistingXTenantHeader()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IBlazorTenantContext>(_ =>
        {
            var ctx = new BlazorTenantContext();
            ctx.Set("from-context");
            return ctx;
        });
        await using var provider = services.BuildServiceProvider();

        var circuitAccessor = new CircuitServicesAccessor { Services = provider };
        var capturing = new CapturingHandler();
        var handler = CreateHandler(circuitAccessor, capturing);

        using var invoker = new HttpMessageInvoker(handler);
        var request = new HttpRequestMessage(HttpMethod.Get, "https://api.example.com/");
        request.Headers.Add(TenantConstants.HeaderName, "pre-set");

        await invoker.SendAsync(request, CancellationToken.None);

        capturing.LastRequest.ShouldNotBeNull();
        capturing.LastRequest!.Headers.GetValues(TenantConstants.HeaderName).Single().ShouldBe("pre-set");
    }

    [Test]
    public async Task SendAsync_DoesNotAddXTenant_WhenUserIsNotAuthenticated()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IBlazorTenantContext>(_ =>
        {
            var ctx = new BlazorTenantContext();
            ctx.Set("acme");
            return ctx;
        });
        await using var provider = services.BuildServiceProvider();

        var circuitAccessor = new CircuitServicesAccessor { Services = provider };
        var capturing = new CapturingHandler();
        var handler = CreateHandlerUnauthenticated(circuitAccessor, capturing);

        using var invoker = new HttpMessageInvoker(handler);
        var request = new HttpRequestMessage(HttpMethod.Get, "https://api.example.com/");
        await invoker.SendAsync(request, CancellationToken.None);

        capturing.LastRequest.ShouldNotBeNull();
        capturing.LastRequest!.Headers.Contains(TenantConstants.HeaderName).ShouldBeFalse();
    }

    private static AccessTokenDelegatingHandler CreateHandler(
        CircuitServicesAccessor circuitAccessor,
        HttpMessageHandler inner)
    {
        var httpContextAccessor = new Mock<IHttpContextAccessor>();
        httpContextAccessor.Setup(x => x.HttpContext).Returns((HttpContext?)null);

        var identity = new ClaimsIdentity(
            new[] { new Claim(AuthConstants.AccessTokenClaim, "test-token") },
            authenticationType: "Test");
        var user = new ClaimsPrincipal(identity);

        var authProvider = new Mock<AuthenticationStateProvider>();
        authProvider
            .Setup(x => x.GetAuthenticationStateAsync())
            .ReturnsAsync(new AuthenticationState(user));

        return new AccessTokenDelegatingHandler(httpContextAccessor.Object, authProvider.Object, circuitAccessor)
        {
            InnerHandler = inner
        };
    }

    private static AccessTokenDelegatingHandler CreateHandlerUnauthenticated(
        CircuitServicesAccessor circuitAccessor,
        HttpMessageHandler inner)
    {
        var httpContextAccessor = new Mock<IHttpContextAccessor>();
        httpContextAccessor.Setup(x => x.HttpContext).Returns((HttpContext?)null);

        var identity = new ClaimsIdentity();
        var user = new ClaimsPrincipal(identity);

        var authProvider = new Mock<AuthenticationStateProvider>();
        authProvider
            .Setup(x => x.GetAuthenticationStateAsync())
            .ReturnsAsync(new AuthenticationState(user));

        return new AccessTokenDelegatingHandler(httpContextAccessor.Object, authProvider.Object, circuitAccessor)
        {
            InnerHandler = inner
        };
    }

    private sealed class CapturingHandler : HttpMessageHandler
    {
        public HttpRequestMessage? LastRequest { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            LastRequest = request;
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
        }
    }
}
