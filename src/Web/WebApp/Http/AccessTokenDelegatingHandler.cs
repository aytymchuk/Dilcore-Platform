using System.Net.Http.Headers;
using Dilcore.MultiTenant.Abstractions;
using Dilcore.WebApp.Constants;
using Dilcore.WebApp.Services;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

namespace Dilcore.WebApp.Http;

/// <summary>
/// DelegatingHandler that adds the access token from the current user's claims to outgoing HTTP requests.
/// Also adds the x-tenant header when a tenant context is available.
/// </summary>
internal class AccessTokenDelegatingHandler : DelegatingHandler
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly AuthenticationStateProvider _authenticationStateProvider;
    private readonly IBlazorTenantAccessor _tenantAccessor;

    public AccessTokenDelegatingHandler(
        IHttpContextAccessor httpContextAccessor,
        AuthenticationStateProvider authenticationStateProvider,
        IBlazorTenantAccessor tenantAccessor)
    {
        _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        _authenticationStateProvider = authenticationStateProvider ?? throw new ArgumentNullException(nameof(authenticationStateProvider));
        _tenantAccessor = tenantAccessor ?? throw new ArgumentNullException(nameof(tenantAccessor));
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var user = await GetAuthenticatedUserAsync();

        if (user?.Identity?.IsAuthenticated == true)
        {
            AddAccessToken(user, request);
            AddTenantHeader(request);
        }

        return await base.SendAsync(request, cancellationToken);
    }

    private async Task<ClaimsPrincipal?> GetAuthenticatedUserAsync()
    {
        var user = _httpContextAccessor.HttpContext?.User;

        if (user?.Identity?.IsAuthenticated == true)
        {
            return user;
        }

        var authState = await _authenticationStateProvider.GetAuthenticationStateAsync();
        return authState.User;
    }

    private void AddTenantHeader(HttpRequestMessage request)
    {
        var tenantSystemName = _tenantAccessor.TenantName;

        if (string.IsNullOrEmpty(tenantSystemName) || request.Headers.Contains(TenantConstants.HeaderName))
        {
            return;
        }

        request.Headers.Add(TenantConstants.HeaderName, tenantSystemName);
    }

    private static void AddAccessToken(ClaimsPrincipal user, HttpRequestMessage request)
    {
        var accessToken = user.FindFirst(AuthConstants.AccessTokenClaim)?.Value;

        if (string.IsNullOrEmpty(accessToken) || request.Headers.Authorization != null)
        {
            return;
        }

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
    }
}
