using Dilcore.FluentValidation.Extensions.MinimalApi;
using Dilcore.Results.Extensions.Api;
using Dilcore.Tenancy.Core.Features.Create;
using Dilcore.Tenancy.Core.Features.Get;
using Dilcore.Tenancy.Core.Features.GetList;
using Finbuckle.MultiTenant.AspNetCore.Extensions;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using ContractTenantDto = Dilcore.Tenancy.Contracts.Tenants.TenantDto;
using CreateTenantDto = Dilcore.Tenancy.Contracts.Tenants.Create.CreateTenantDto;

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
            var command = new CreateTenantCommand(request.Name, request.Description);
            var result = await mediator.Send(command, cancellationToken);
            if (result.IsSuccess)
            {
                var response = new ContractTenantDto
                {
                    Name = result.Value.Name,
                    SystemName = result.Value.SystemName,
                    Description = result.Value.Description,
                    CreatedAt = result.Value.CreatedAt
                };
                return Microsoft.AspNetCore.Http.Results.Created($"/tenants", response);
            }
            return result.ToMinimalApiResult();
        })
        .WithName("CreateTenant")
        .Produces<ContractTenantDto>()
        .AddValidationFilter<CreateTenantDto>()
        .ProducesValidationProblem()
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ExcludeFromMultiTenantResolution();

        // GET /tenants - Get list of user's tenants (user-scoped, no x-tenant header)
        group.MapGet("/", async (
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            var query = new GetTenantsListQuery();
            var result = await mediator.Send(query, cancellationToken);
            return result.Map(tenants => tenants.Select(t => new ContractTenantDto
            {
                Name = t.Name,
                SystemName = t.SystemName,
                Description = t.Description,
                CreatedAt = t.CreatedAt
            }).ToList()).ToMinimalApiResult();
        })
        .WithName("GetTenantsList")
        .Produces<List<ContractTenantDto>>()
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ExcludeFromMultiTenantResolution();

        // GET /tenants/current - Get current tenant (uses x-tenant header)
        group.MapGet("/current", async (
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            var query = new GetTenantQuery();
            var result = await mediator.Send(query, cancellationToken);
            return result.Map(v => new ContractTenantDto
            {
                Name = v.Name,
                SystemName = v.SystemName,
                Description = v.Description,
                CreatedAt = v.CreatedAt
            }).ToMinimalApiResult();
        })
        .WithName("GetCurrentTenant")
        .Produces<ContractTenantDto>()
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status401Unauthorized);
    }
}
