using Dilcore.MediatR.Abstractions;
using Dilcore.WebApp.Http.AiAgent;
using Dilcore.WebApp.Http.AiAgent.Dtos;
using Dilcore.WebApp.Http.AiAgent.Extensions;
using FluentResults;

namespace Dilcore.WebApp.Features.Tenants.Admin.Agent.Queries;

public class GetAgentThreadQueryHandler : IQueryHandler<GetAgentThreadQuery, ThreadActionResponseDto>
{
    private readonly IBlueprintsAgentClient _client;
    private readonly AgentApiSettings _settings;

    public GetAgentThreadQueryHandler(IBlueprintsAgentClient client, AgentApiSettings settings)
    {
        _client = client;
        _settings = settings;
    }

    public async Task<Result<ThreadActionResponseDto>> Handle(
        GetAgentThreadQuery request,
        CancellationToken cancellationToken)
    {
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        cts.CancelAfter(_settings.Timeout);
        return await _client.SafeGetThreadAsync(request.ThreadId, cts.Token);
    }
}
