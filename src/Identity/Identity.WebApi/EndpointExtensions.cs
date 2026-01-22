using Dilcore.FluentValidation.Extensions.MinimalApi;
using Dilcore.Identity.Contracts.Profile;
using Dilcore.Identity.Contracts.Register;
using Dilcore.Identity.Core.Features.GetCurrent;
using Dilcore.Identity.Core.Features.Register;
using Dilcore.Results.Extensions.Api;
using Finbuckle.MultiTenant.AspNetCore.Extensions;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using HttpResults = Microsoft.AspNetCore.Http.Results;

namespace Dilcore.Identity.WebApi;

/// <summary>
/// HTTP endpoints for the Identity module.
/// </summary>
public static class EndpointExtensions
{
    /// <summary>
    /// Maps all Identity module endpoints.
    /// </summary>
    public static void MapIdentityEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/users")
            .WithTags("Users")
            .ExcludeFromMultiTenantResolution();

        // POST /users/register - Register the current user
        group.MapPost("/register", async (
            RegisterUserDto dto,
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            var command = new RegisterUserCommand(dto.Email, dto.FirstName, dto.LastName);
            var result = await mediator.Send(command, cancellationToken);

            if (result.IsFailed)
            {
                return result.ToMinimalApiResult();
            }

            var userResponse = result.Value;
            return HttpResults.Ok(new UserDto(
                userResponse.Id,
                userResponse.Email,
                userResponse.FirstName,
                userResponse.LastName,
                userResponse.RegisteredAt));
        })
        .WithName("RegisterUser")
        .AddValidationFilter<RegisterUserDto>()
        .Produces<UserDto>()
        .ProducesValidationProblem()
        .ProducesProblem(StatusCodes.Status401Unauthorized);

        // GET /users/me - Get current user profile
        group.MapGet("/me", async (
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            var result = await mediator.Send(new GetCurrentUserQuery(), cancellationToken);

            if (result.IsFailed)
            {
                return result.ToMinimalApiResult();
            }

            var userResponse = result.Value;
            if (userResponse is null)
            {
                return HttpResults.NotFound();
            }

            return HttpResults.Ok(new UserDto(
                userResponse.Id,
                userResponse.Email,
                userResponse.FirstName,
                userResponse.LastName,
                userResponse.RegisteredAt));
        })
        .WithName("GetCurrentUser")
        .Produces<UserDto>()
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status401Unauthorized);
    }
}