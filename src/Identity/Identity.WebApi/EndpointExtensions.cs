using Dilcore.Identity.Actors.Abstractions;
using Dilcore.Identity.Core.Features.GetCurrent;
using Dilcore.Identity.Core.Features.Register;
using Dilcore.Results.Extensions.Api;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

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
            .RequireAuthorization();

        // POST /users/register - Register the current user
        group.MapPost("/register", async (
            RegisterUserCommand command,
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            var result = await mediator.Send(command, cancellationToken);
            return result.ToMinimalApiResult();
        })
        .WithName("RegisterUser")
        .Produces<UserDto>()
        .ProducesValidationProblem()
        .ProducesProblem(StatusCodes.Status401Unauthorized);

        // GET /users/me - Get current user profile
        group.MapGet("/me", async (
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            var result = await mediator.Send(new GetCurrentUserQuery(), cancellationToken);
            return result.ToMinimalApiResult();
        })
        .WithName("GetCurrentUser")
        .Produces<UserDto>()
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status401Unauthorized);
    }
}