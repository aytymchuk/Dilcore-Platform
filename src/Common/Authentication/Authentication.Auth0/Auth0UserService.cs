using System.Net.Http.Json;
using Microsoft.Extensions.Logging;

namespace Dilcore.Authentication.Auth0;

/// <summary>
/// Service for retrieving user information from Auth0 UserInfo endpoint.
/// </summary>
public sealed class Auth0UserService : IAuth0UserService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<Auth0UserService> _logger;

    public Auth0UserService(HttpClient httpClient, ILogger<Auth0UserService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<Auth0UserProfile?> GetUserProfileAsync(string accessToken, CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Get, "userinfo");
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

            var response = await _httpClient.SendAsync(request, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogAuth0UserInfoFailed((int)response.StatusCode, response.ReasonPhrase ?? "Unknown");
                return null;
            }

            var profile = await response.Content.ReadFromJsonAsync<Auth0UserProfile>(cancellationToken);
            _logger.LogAuth0UserInfoSuccess(profile?.Sub);
            return profile;
        }
        catch (Exception ex)
        {
            _logger.LogAuth0UserInfoError(ex);
            return null;
        }
    }
}