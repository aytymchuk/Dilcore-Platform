using Dilcore.Authentication.Abstractions;
using Dilcore.Identity.Actors.Abstractions;
using Dilcore.MediatR.Abstractions;
using Dilcore.Results.Abstractions;
using FluentResults;

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

        if (!result.IsSuccess)
        {
            return Result.Fail(new ConflictError(result.ErrorMessage ?? "Failed to register user."));
        }

        if (result.User is null)
        {
            return Result.Fail<UserDto>("User registration succeeded but returned null.");
        }

        return Result.Ok(result.User);
    }
}