using System.Net;
using Dilcore.Identity.Contracts.Profile;
using Dilcore.MediatR.Abstractions;
using Dilcore.WebApi.Client.Clients;
using Dilcore.WebApi.Client.Errors;
using Dilcore.WebApi.Client.Extensions;
using FluentResults;

namespace Dilcore.WebApp.Features.Users.CurrentUser;

/// <summary>
/// Handler for getting the current user via the Platform API.
/// </summary>
internal sealed class GetCurrentUserQueryHandler : IQueryHandler<GetCurrentUserQuery, UserDto?>
{
    private readonly IIdentityClient _identityClient;

    public GetCurrentUserQueryHandler(IIdentityClient identityClient)
    {
        _identityClient = identityClient ?? throw new ArgumentNullException(nameof(identityClient));
    }

    public async Task<Result<UserDto?>> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
    {
        var result = await _identityClient.SafeGetCurrentUserAsync(cancellationToken);

        if (result.IsSuccess)
        {
            return Result.Ok<UserDto?>(result.Value);
        }

        // Check if the error is a 404 (user not found)
        var apiError = result.Errors.OfType<ApiError>().FirstOrDefault();
        if (apiError?.StatusCode == (int)HttpStatusCode.NotFound)
        {
            return Result.Fail<UserDto?>(new UserNotFoundError());
        }

        // Re-wrap other errors
        return Result.Fail<UserDto?>(result.Errors);
    }
}
