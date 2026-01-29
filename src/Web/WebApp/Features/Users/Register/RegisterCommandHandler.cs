using Dilcore.Identity.Contracts.Register;
using Dilcore.MediatR.Abstractions;
using Dilcore.WebApi.Client.Clients;
using Dilcore.WebApi.Client.Extensions;
using Dilcore.WebApp.Models.Users;
using FluentResults;

namespace Dilcore.WebApp.Features.Users.Register;

/// <summary>
/// Handler for registering the current user via the Platform API.
/// </summary>
internal sealed class RegisterCommandHandler : ICommandHandler<RegisterCommand, UserModel>
{
    private readonly IIdentityClient _identityClient;

    public RegisterCommandHandler(IIdentityClient identityClient)
    {
        _identityClient = identityClient ?? throw new ArgumentNullException(nameof(identityClient));
    }

    public async Task<Result<UserModel>> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var parameters = request.Parameters;
        var dto = new RegisterUserDto(parameters.Email, parameters.FirstName, parameters.LastName);
        var result = await _identityClient.SafeRegisterUserAsync(dto, cancellationToken);

        return result.Map(user => new UserModel(
            user.Id,
            user.Email,
            user.FirstName,
            user.LastName));
    }
}
