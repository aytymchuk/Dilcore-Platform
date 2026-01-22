using Dilcore.Authentication.Abstractions;
using Dilcore.Identity.Actors.Abstractions;
using Dilcore.MediatR.Abstractions;
using Dilcore.Results.Abstractions;
using FluentResults;

namespace Dilcore.Identity.Core.Features.GetCurrent;

/// <summary>
/// Handles getting the current user's profile via the UserGrain.
/// </summary>
public sealed class GetCurrentUserHandler : IQueryHandler<GetCurrentUserQuery, UserResponse?>
{
    private readonly IUserContextResolver _userContextResolver;
    private readonly IGrainFactory _grainFactory;

    public GetCurrentUserHandler(IUserContextResolver userContextResolver, IGrainFactory grainFactory)
    {
        _userContextResolver = userContextResolver;
        _grainFactory = grainFactory;
    }

    public async Task<Result<UserResponse?>> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
    {
        var userContext = _userContextResolver.Resolve();

        if (userContext.Id is null)
        {
            return Result.Fail<UserResponse?>(new ValidationError("User ID is required"));
        }

        var grain = _grainFactory.GetGrain<IUserGrain>(userContext.Id);
        var result = await grain.GetProfileAsync();

        if (result is null)
        {
            return Result.Fail<UserResponse?>(new NotFoundError("User", userContext.Id!));
        }

        return Result.Ok<UserResponse?>(result);
    }
}