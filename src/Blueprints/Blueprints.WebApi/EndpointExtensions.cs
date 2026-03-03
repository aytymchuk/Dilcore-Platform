using AutoMapper;
using Dilcore.Blueprints.Contracts;
using Dilcore.Blueprints.Contracts.EntityDefinitions;
using Dilcore.Blueprints.Contracts.EntityDefinitions.Create;
using Dilcore.Blueprints.Contracts.EntityDefinitions.Update;
using Dilcore.Blueprints.Core.Features.EntityDefinitions.Create;
using Dilcore.Blueprints.Core.Features.EntityDefinitions.Delete;
using Dilcore.Blueprints.Core.Features.EntityDefinitions.GetById;
using Dilcore.Blueprints.Core.Features.EntityDefinitions.GetList;
using Dilcore.Blueprints.Core.Features.EntityDefinitions.Update;
using Dilcore.FluentValidation.Extensions.MinimalApi;
using Dilcore.Results.Extensions.Api;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Dilcore.Blueprints.WebApi;

public static class EndpointExtensions
{
    public static void MapBlueprintsEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/blueprints")
            .WithTags("Blueprints")
            .RequireAuthorization();

        MapEntityDefinitionEndpoints(group);
    }

    private static void MapEntityDefinitionEndpoints(RouteGroupBuilder group)
    {
        var entities = group.MapGroup("/entity-definitions");

        entities.MapGet("/", async Task<IResult> (
            int? skip,
            int? take,
            string? search,
            bool? isAbstract,
            string? tags,
            IMediator mediator,
            IMapper mapper,
            CancellationToken ct) =>
        {
            const int maxTake = 100;
            var effectiveSkip = skip ?? 0;
            var effectiveTake = take ?? 20;

            if (effectiveSkip < 0)
                return Microsoft.AspNetCore.Http.Results.BadRequest(
                    new { field = "skip", error = "Must be >= 0." });
            if (effectiveTake < 1 || effectiveTake > maxTake)
                return Microsoft.AspNetCore.Http.Results.BadRequest(
                    new { field = "take", error = $"Must be between 1 and {maxTake}." });

            var tagList = string.IsNullOrWhiteSpace(tags)
                ? null
                : tags.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                    .ToList() as IReadOnlyList<string>;

            var query = new GetEntityDefinitionsQuery
            {
                Skip = effectiveSkip,
                Take = effectiveTake,
                SearchTerm = search,
                IsAbstract = isAbstract,
                Tags = tagList
            };

            var result = await mediator.Send(query, ct);
            return result
                .Map(paged => new PagedResult<EntityDefinitionDto>
                {
                    Items = mapper.Map<List<EntityDefinitionDto>>(paged.Items),
                    TotalCount = paged.TotalCount
                })
                .ToMinimalApiResult();
        })
        .WithName("GetEntityDefinitions")
        .Produces<PagedResult<EntityDefinitionDto>>()
        .ProducesProblem(StatusCodes.Status401Unauthorized);

        entities.MapGet("/{id:guid}", async (
            Guid id,
            IMediator mediator,
            IMapper mapper,
            CancellationToken ct) =>
        {
            var result = await mediator.Send(new GetEntityDefinitionQuery(id), ct);
            return result
                .Map(mapper.Map<EntityDefinitionDto>)
                .ToMinimalApiResult();
        })
        .WithName("GetEntityDefinition")
        .Produces<EntityDefinitionDto>()
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status401Unauthorized);

        entities.MapPost("/", async (
            CreateEntityDefinitionDto request,
            IMediator mediator,
            IMapper mapper,
            CancellationToken ct) =>
        {
            var command = mapper.Map<CreateEntityDefinitionCommand>(request);
            var result = await mediator.Send(command, ct);
            return result
                .Map(mapper.Map<EntityDefinitionDto>)
                .ToMinimalApiResult(dto =>
                    Microsoft.AspNetCore.Http.Results.Created($"/blueprints/entity-definitions/{dto.Id}", dto));
        })
        .WithName("CreateEntityDefinition")
        .Produces<EntityDefinitionDto>(StatusCodes.Status201Created)
        .AddValidationFilter<CreateEntityDefinitionDto>()
        .ProducesValidationProblem()
        .ProducesProblem(StatusCodes.Status409Conflict)
        .ProducesProblem(StatusCodes.Status401Unauthorized);

        entities.MapPut("/{id:guid}", async (
            Guid id,
            UpdateEntityDefinitionDto request,
            IMediator mediator,
            IMapper mapper,
            CancellationToken ct) =>
        {
            var command = mapper.Map<UpdateEntityDefinitionCommand>(request) with { Id = id };
            var result = await mediator.Send(command, ct);
            return result
                .Map(mapper.Map<EntityDefinitionDto>)
                .ToMinimalApiResult();
        })
        .WithName("UpdateEntityDefinition")
        .Produces<EntityDefinitionDto>()
        .AddValidationFilter<UpdateEntityDefinitionDto>()
        .ProducesValidationProblem()
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status401Unauthorized);

        entities.MapDelete("/{id:guid}", async (
            Guid id,
            IMediator mediator,
            CancellationToken ct) =>
        {
            var result = await mediator.Send(new DeleteEntityDefinitionCommand(id), ct);
            return result.ToMinimalApiResult();
        })
        .WithName("DeleteEntityDefinition")
        .Produces(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status401Unauthorized);
    }
}
