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
            AutoMapper.IMapper mapper,
            CancellationToken cancellationToken) =>
        {
            var command = new CreateTenantCommand(request.Name, request.Description);
            
            var result = await mediator.Send(command, cancellationToken);
            return result
                .Map(mapper.Map<ContractTenantDto>)
                .ToMinimalApiResult(tenant => Microsoft.AspNetCore.Http.Results.Created($"/tenants/current", tenant));
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
            AutoMapper.IMapper mapper,
            CancellationToken cancellationToken) =>
        {
            var query = new GetTenantsListQuery();
            var result = await mediator.Send(query, cancellationToken);
            return result.Map(tenants => mapper.Map<IEnumerable<ContractTenantDto>>(tenants)).ToMinimalApiResult();
        })
        .WithName("GetTenantsList")
        .Produces<IEnumerable<ContractTenantDto>>()
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ExcludeFromMultiTenantResolution();

        // GET /tenants/current - Get current tenant (uses x-tenant header)
        group.MapGet("/current", async (
            IMediator mediator,
            AutoMapper.IMapper mapper,
            CancellationToken cancellationToken) =>
        {
            var query = new GetTenantQuery();
            var result = await mediator.Send(query, cancellationToken);
            return result.Map(t => mapper.Map<ContractTenantDto>(t)).ToMinimalApiResult();
        })
        .WithName("GetCurrentTenant")
        .Produces<ContractTenantDto>()
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status401Unauthorized);
    }
}