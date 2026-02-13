using Dilcore.Authentication.Abstractions;
using Dilcore.Identity.Actors.Abstractions;
using Dilcore.Tenancy.Domain;
using FluentResults;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Dilcore.Tenancy.Core.Features.Create.Behaviors;

public sealed class VerifyUserRegisteredBehavior : IPipelineBehavior<CreateTenantCommand, Result<Tenant>>
{
    private readonly IUserContext _userContext;
    private readonly IGrainFactory _grainFactory;
    private readonly ILogger<VerifyUserRegisteredBehavior> _logger;

    public VerifyUserRegisteredBehavior(
        IUserContext userContext,
        IGrainFactory grainFactory,
        ILogger<VerifyUserRegisteredBehavior> logger)
    {
        _userContext = userContext;
        _grainFactory = grainFactory;
        _logger = logger;
    }

    public async Task<Result<Tenant>> Handle(
        CreateTenantCommand request,
        RequestHandlerDelegate<Result<Tenant>> next,
        CancellationToken cancellationToken)
    {
        var userGrain = _grainFactory.GetGrain<IUserGrain>(_userContext.Id);
        var isRegistered = await userGrain.IsRegisteredAsync();

        if (!isRegistered)
        {
            _logger.LogUserNotRegisteredForTenantCreation(_userContext.Id);
            return Result.Fail<Tenant>("User is not registered.");
        }

        return await next();
    }
}
