using Dilcore.Identity.Actors.Abstractions;
using Dilcore.Identity.Core.Abstractions;
using Dilcore.Results.Abstractions;
using FluentResults;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Dilcore.Identity.Core.Features.Register.Behaviors;

public class ValidateUserEmailBehavior : IPipelineBehavior<RegisterUserCommand, Result<UserResponse>>
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<ValidateUserEmailBehavior> _logger;

    public ValidateUserEmailBehavior(IUserRepository userRepository, ILogger<ValidateUserEmailBehavior> logger)
    {
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task<Result<UserResponse>> Handle(RegisterUserCommand request, RequestHandlerDelegate<Result<UserResponse>> next, CancellationToken cancellationToken)
    {
        var userResult = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);

        if (userResult.IsFailed)
        {
            return Result.Fail(userResult.Errors);
        }

        if (userResult.Value is not null)
        {
            _logger.LogUserAlreadyExists(request.Email);
            return Result.Fail(new ConflictError($"User with email '{request.Email}' already exists.")
                .WithMetadata("UserId", userResult.Value.Id));
        }

        return await next(cancellationToken);
    }
}