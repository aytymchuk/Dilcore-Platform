using Dilcore.Authentication.Abstractions;
using Dilcore.Identity.Actors.Abstractions;
using Dilcore.MediatR.Abstractions;
using FluentResults;

namespace Dilcore.Identity.Core.Features.GetCurrent;

/// <summary>
/// Handles getting the current user's profile via the UserGrain.
/// </summary>
public sealed class GetCurrentUserHandler : IQueryHandler<GetCurrentUserQuery, UserDto?>
{
    private readonly IUserContext _userContext;
    private readonly IGrainFactory _grainFactory;

    public GetCurrentUserHandler(IUserContext userContext, IGrainFactory grainFactory)
    {
        _userContext = userContext;
        _grainFactory = grainFactory;
    }

    public async Task<Result<UserDto?>> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
    {
        if (_userContext.Id is null)
        {
            return Result.Fail<UserDto?>("User ID is required");
        }

        var grain = _grainFactory.GetGrain<IUserGrain>(_userContext.Id);
        var result = await grain.GetProfileAsync();
        return Result.Ok(result);
    }
}
