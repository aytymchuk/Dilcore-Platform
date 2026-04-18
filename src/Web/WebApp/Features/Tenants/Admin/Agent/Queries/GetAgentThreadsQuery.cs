using Dilcore.MediatR.Abstractions;
using Dilcore.WebApp.Http.AiAgent.Dtos;

namespace Dilcore.WebApp.Features.Tenants.Admin.Agent.Queries;

public record GetAgentThreadsQuery : IQuery<IReadOnlyList<ThreadResponseDto>>;
