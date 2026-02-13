using System.Net.Http.Headers;
using Dilcore.MultiTenant.Abstractions;
using Dilcore.WebApp.Constants;
using Dilcore.WebApp.Routing;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

namespace Dilcore.WebApp.Http;

/// <summary>
/// DelegatingHandler that adds the access token from the current user's claims to outgoing HTTP requests.
/// Also adds the x-tenant header when a tenant context is available.
/// </summary>
internal sealed class AccessTokenDelegatingHandler : DelegatingHandler
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly AuthenticationStateProvider _authenticationStateProvider;
    private readonly NavigationManager _navigationManager;

    public AccessTokenDelegatingHandler(
        IHttpContextAccessor httpContextAccessor,
        AuthenticationStateProvider authenticationStateProvider,
        NavigationManager navigationManager)
    {
        _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        _authenticationStateProvider = authenticationStateProvider ?? throw new ArgumentNullException(nameof(authenticationStateProvider));
        _navigationManager = navigationManager ?? throw new ArgumentNullException(nameof(navigationManager));
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var user = _httpContextAccessor.HttpContext?.User;

        // Fallback to AuthenticationStateProvider for Blazor Server interactive components
        if (user?.Identity?.IsAuthenticated != true)
        {
            var authState = await _authenticationStateProvider.GetAuthenticationStateAsync();
            user = authState.User;
        }

        if (user?.Identity?.IsAuthenticated == true)
        {
            AddAccessToken(user, request);
            AddTenantHeader(user, request);
        }

        return await base.SendAsync(request, cancellationToken);
    }

    private static void AddAccessToken(ClaimsPrincipal user, HttpRequestMessage request)
    {
        var accessToken = user.FindFirst(AuthConstants.AccessTokenClaim)?.Value;

        if (!string.IsNullOrEmpty(accessToken) && request.Headers.Authorization == null)
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        }
    }

    private void AddTenantHeader(ClaimsPrincipal user, HttpRequestMessage request)
    {
        // Try to get tenant from HttpContext path first
        var path = _httpContextAccessor.HttpContext?.Request.Path.Value;
        
        // Fallback to NavigationManager for Blazor Server
        if (string.IsNullOrEmpty(path))
        {
            try
            {
                var uri = new Uri(_navigationManager.Uri);
                path = uri.AbsolutePath;
            }
            catch
            {
                // Ignore parsing errors
            }
        }

        var tenantSystemName = !string.IsNullOrEmpty(path) 
            ? TenantRouteHelper.ExtractTenantFromPath(path)
            : null;

        if (!string.IsNullOrEmpty(tenantSystemName) && !request.Headers.Contains(TenantConstants.HeaderName))
        {
            request.Headers.Add(TenantConstants.HeaderName, tenantSystemName);
        }
    }
}
