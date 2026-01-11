using Dilcore.Authentication.Abstractions;
using Dilcore.Results.Abstractions;
using Dilcore.Identity.Actors.Abstractions;
using FluentResults;
using MediatR;
using Dilcore.MediatR.Abstractions;

namespace Dilcore.Identity.Core.Features.Register;

/// <summary>
/// Handles user registration by invoking the UserGrain.
/// </summary>
public sealed class RegisterUserHandler : ICommandHandler<RegisterUserCommand, UserDto>
{
    private readonly IUserContextResolver _userContextResolver;
    private readonly IGrainFactory _grainFactory;

    public RegisterUserHandler(IUserContextResolver userContextResolver, IGrainFactory grainFactory)
    {
        _userContextResolver = userContextResolver ?? throw new ArgumentNullException(nameof(userContextResolver));
        _grainFactory = grainFactory ?? throw new ArgumentNullException(nameof(grainFactory));
    }

    public async Task<Result<UserDto>> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        var userContext = _userContextResolver.Resolve();

        if (userContext.Id is null)
        {
            return Result.Fail<UserDto>(new ValidationError("User ID is required for registration"));
        }

        var grain = _grainFactory.GetGrain<IUserGrain>(userContext.Id);
        var result = await grain.RegisterAsync(request.Email, request.FullName);
        return Result.Ok(result);
    }
}