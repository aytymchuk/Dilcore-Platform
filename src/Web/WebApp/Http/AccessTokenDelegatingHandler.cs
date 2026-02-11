using System.Net.Http.Headers;
using Dilcore.MultiTenant.Abstractions;
using Dilcore.WebApp.Constants;
using Dilcore.WebApp.Routing;

namespace Dilcore.WebApp.Http;

/// <summary>
/// DelegatingHandler that adds the access token from the current user's claims to outgoing HTTP requests.
/// Also adds the x-tenant header when a tenant context is available.
/// </summary>
internal sealed class AccessTokenDelegatingHandler : DelegatingHandler
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AccessTokenDelegatingHandler(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var httpContext = _httpContextAccessor.HttpContext;

        if (httpContext?.User?.Identity?.IsAuthenticated == true)
        {
            AddAccessToken(httpContext, request);
            AddTenantHeader(httpContext, request);
        }

        return await base.SendAsync(request, cancellationToken);
    }

    private static void AddAccessToken(HttpContext httpContext, HttpRequestMessage request)
    {
        var accessToken = httpContext.User.FindFirst(AuthConstants.AccessTokenClaim)?.Value;

        if (!string.IsNullOrEmpty(accessToken) && request.Headers.Authorization == null)
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        }
    }

    private static void AddTenantHeader(HttpContext httpContext, HttpRequestMessage request)
    {
        var tenantSystemName = TenantRouteHelper.ExtractTenantFromPath(httpContext.Request.Path.Value);

        if (!string.IsNullOrEmpty(tenantSystemName) && !request.Headers.Contains(TenantConstants.HeaderName))
        {
            request.Headers.Add(TenantConstants.HeaderName, tenantSystemName);
        }
    }
}
