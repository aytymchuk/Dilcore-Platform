using Dilcore.Results.Extensions.Api;
using Dilcore.WebApi.Features.GitHub;
using MediatR;

namespace Dilcore.WebApi.Endpoints;

public static class GitHubEndpoints
{
    public static RouteGroupBuilder MapGitHubEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("/{username}", GetGitHubUser)
            .WithName("GetGitHubUser")
            .WithSummary("Get GitHub user information")
            .WithDescription("Fetches public information about a GitHub user. Used to test HTTP client tracing.")
            .Produces<GitHubUserResponse>()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        return group;
    }

    private static async Task<IResult> GetGitHubUser(
        string username,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetGitHubUserQuery(username), cancellationToken);
        return result.ToMinimalApiResult();
    }
}
