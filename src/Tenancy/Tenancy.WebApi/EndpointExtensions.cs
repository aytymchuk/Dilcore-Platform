using Dilcore.Results.Extensions.Api;
using Dilcore.Tenancy.Contracts.Tenants;
using CreateTenantDto = Dilcore.Tenancy.Contracts.Tenants.Create.CreateTenantDto;
using ContractTenantDto = Dilcore.Tenancy.Contracts.Tenants.TenantDto;
using Dilcore.Tenancy.Actors.Abstractions;
using Dilcore.Tenancy.Core.Features.Create;
using Dilcore.Tenancy.Core.Features.Get;
using Finbuckle.MultiTenant.AspNetCore.Extensions;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Dilcore.Tenancy.WebApi;

/// <summary>
/// HTTP endpoints for the Tenancy module.
/// </summary>
public static class EndpointExtensions
{
    /// <summary>
    /// Maps all Tenancy module endpoints.
    /// </summary>
    public static void MapTenancyEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/tenants")
            .WithTags("Tenants")
            .RequireAuthorization();

        // POST /tenants - Create a new tenant (not tenant-specific)
        group.MapPost("/", async (
            CreateTenantDto request,
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            var command = new CreateTenantCommand(request.DisplayName, request.Description);
            var result = await mediator.Send(command, cancellationToken);
            if (result.IsSuccess)
            {
                return Microsoft.AspNetCore.Http.Results.Created($"/tenants", result.Value);
            }
            return result.ToMinimalApiResult();
        })
        .WithName("CreateTenant")
        .Produces<ContractTenantDto>()
        .ProducesValidationProblem()
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ExcludeFromMultiTenantResolution();

        // GET /tenants - Get current tenant (uses x-tenant header)
        group.MapGet("/", async (
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            var result = await mediator.Send(new GetTenantQuery(), cancellationToken);
            return result.ToMinimalApiResult();
        })
        .WithName("GetTenant")
        .Produces<ContractTenantDto>()
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status401Unauthorized);
    }
}
