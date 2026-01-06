using System.Text.Json.Serialization;
using FluentResults;
using MediatR;

namespace Dilcore.WebApi.Features.GitHub;

public class GetGitHubUserHandler : IRequestHandler<GetGitHubUserQuery, Result<GitHubUserResponse>>
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<GetGitHubUserHandler> _logger;

    public GetGitHubUserHandler(
        IHttpClientFactory httpClientFactory,
        ILogger<GetGitHubUserHandler> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task<Result<GitHubUserResponse>> Handle(
        GetGitHubUserQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Add("User-Agent", "Dilcore-Platform");

            var response = await client.GetAsync(
                $"https://api.github.com/users/{request.Username}",
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return Result.Fail($"GitHub API returned {response.StatusCode}");
            }

            var githubUser = await response.Content.ReadFromJsonAsync<GitHubApiUser>(cancellationToken);

            if (githubUser == null)
            {
                return Result.Fail("Failed to deserialize GitHub response");
            }

            return Result.Ok(new GitHubUserResponse(
                githubUser.Login,
                githubUser.Name ?? githubUser.Login,
                githubUser.Bio,
                githubUser.PublicRepos,
                githubUser.Followers,
                githubUser.Following));
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP request to GitHub failed for user {Username}", request.Username);
            return Result.Fail($"Failed to fetch GitHub user: {ex.Message}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error fetching GitHub user {Username}", request.Username);
            return Result.Fail($"Unexpected error: {ex.Message}");
        }
    }

    private record GitHubApiUser(
        [property: JsonPropertyName("login")] string Login,
        [property: JsonPropertyName("name")] string? Name,
        [property: JsonPropertyName("bio")] string? Bio,
        [property: JsonPropertyName("public_repos")] int PublicRepos,
        [property: JsonPropertyName("followers")] int Followers,
        [property: JsonPropertyName("following")] int Following);
}
