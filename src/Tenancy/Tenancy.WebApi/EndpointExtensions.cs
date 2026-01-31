using Dilcore.FluentValidation.Extensions.MinimalApi;
using Dilcore.Results.Extensions.Api;
using CreateTenantDto = Dilcore.Tenancy.Contracts.Tenants.Create.CreateTenantDto;
using ContractTenantDto = Dilcore.Tenancy.Contracts.Tenants.TenantDto;
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

        // GET /tenants - Get current tenant (uses x-tenant header)
        group.MapGet("/", async (
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            var query = new GetTenantQuery(); // Restored missing command/query variable
            var result = await mediator.Send(query, cancellationToken);
            return result.Map(v => new ContractTenantDto
            {
                Name = v.Name,
                SystemName = v.SystemName,
                Description = v.Description,
                CreatedAt = v.CreatedAt
            }).ToMinimalApiResult();
        })
        .WithName("GetTenant")
        .Produces<ContractTenantDto>()
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status401Unauthorized);
    }
}
