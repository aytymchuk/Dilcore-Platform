using Dilcore.MediatR.Abstractions;
using Dilcore.WebApp.Http.AiAgent;
using Dilcore.WebApp.Http.AiAgent.Dtos;
using Dilcore.WebApp.Http.AiAgent.Extensions;
using FluentResults;

namespace Dilcore.WebApp.Features.Tenants.Admin.Agent.Queries;

public class GetAgentThreadsQueryHandler : IQueryHandler<GetAgentThreadsQuery, IReadOnlyList<ThreadResponseDto>>
{
    private readonly IBlueprintsAgentClient _client;

    public GetAgentThreadsQueryHandler(IBlueprintsAgentClient client)
    {
        _client = client;
    }

    public async Task<Result<IReadOnlyList<ThreadResponseDto>>> Handle(
        GetAgentThreadsQuery request,
        CancellationToken cancellationToken)
    {
        return await _client.SafeGetThreadsAsync(cancellationToken);
    }
}
