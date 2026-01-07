using Dilcore.Authentication.Abstractions;
using Dilcore.Identity.Actors.Abstractions;
using FluentResults;
using MediatR;

namespace Dilcore.Identity.Core.Features.Register;

/// <summary>
/// Handles user registration by invoking the UserGrain.
/// </summary>
public sealed class RegisterUserHandler : IRequestHandler<RegisterUserCommand, Result<UserDto>>
{
    private readonly IUserContext _userContext;
    private readonly IGrainFactory _grainFactory;

    public RegisterUserHandler(IUserContext userContext, IGrainFactory grainFactory)
    {
        _userContext = userContext;
        _grainFactory = grainFactory;
    }

    public async Task<Result<UserDto>> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        if (_userContext.Id is null)
        {
            return Result.Fail<UserDto>("User ID is required for registration");
        }

        var grain = _grainFactory.GetGrain<IUserGrain>(_userContext.Id);
        var result = await grain.RegisterAsync(request.Email, request.FullName);
        return Result.Ok(result);
    }
}
