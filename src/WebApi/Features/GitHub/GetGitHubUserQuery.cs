using FluentResults;
using MediatR;

namespace Dilcore.WebApi.Features.GitHub;

public record GetGitHubUserQuery(string Username) : IRequest<Result<GitHubUserResponse>>;

public record GitHubUserResponse(
    string Login,
    string Name,
    string? Bio,
    int PublicRepos,
    int Followers,
    int Following);
