using Dilcore.MediatR.Abstractions;
using Dilcore.WebApp.Models.Users;
using Dilcore.WebApi.Client.Clients;
using Dilcore.WebApi.Client.Errors;
using Dilcore.WebApi.Client.Extensions;
using FluentResults;

namespace Dilcore.WebApp.Features.Users.CurrentUser;

/// <summary>
/// Handler for getting the current user via the Platform API.
/// </summary>
internal sealed class GetCurrentUserQueryHandler(IIdentityClient identityClient)
    : IQueryHandler<GetCurrentUserQuery, UserModel?>
{
    private readonly IIdentityClient _identityClient = identityClient ?? throw new ArgumentNullException(nameof(identityClient));

    public async Task<Result<UserModel?>> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
    {
        var result = await _identityClient.SafeGetCurrentUserAsync(cancellationToken);

        if (result.IsSuccess)
        {
            return result.Map(dto => (UserModel?)UserModel.FromDto(dto));
        }

        // Check if the error is a 404 (user not found)
        var apiError = result.Errors.OfType<ApiError>().FirstOrDefault();
        
        return apiError?.NotFound is true 
            ? Result.Ok<UserModel?>(null) 
            : result.ToResult();
    }
}