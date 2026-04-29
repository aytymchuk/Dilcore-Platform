using Dilcore.MediatR.Abstractions;
using Dilcore.WebApp.Http.AiAgent;
using Dilcore.WebApp.Http.AiAgent.Dtos;
using Dilcore.WebApp.Http.AiAgent.Extensions;
using FluentResults;

namespace Dilcore.WebApp.Features.Tenants.Admin.Agent.Queries;

public class GetAgentThreadsQueryHandler : IQueryHandler<GetAgentThreadsQuery, IReadOnlyList<ThreadResponseDto>>
{
    private readonly IBlueprintsAgentClient _client;
    private readonly AgentApiSettings _settings;

    public GetAgentThreadsQueryHandler(IBlueprintsAgentClient client, AgentApiSettings settings)
    {
        _client = client;
        _settings = settings;
    }

    public async Task<Result<IReadOnlyList<ThreadResponseDto>>> Handle(
        GetAgentThreadsQuery request,
        CancellationToken cancellationToken)
    {
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        cts.CancelAfter(_settings.Timeout);
        return await _client.SafeGetThreadsAsync(cts.Token);
    }
}
