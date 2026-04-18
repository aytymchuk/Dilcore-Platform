using Dilcore.MediatR.Abstractions;
using Dilcore.WebApp.Http.AiAgent;
using Dilcore.WebApp.Http.AiAgent.Dtos;
using Dilcore.WebApp.Http.AiAgent.Extensions;
using FluentResults;

namespace Dilcore.WebApp.Features.Tenants.Admin.Agent.Queries;

public class GetAgentThreadQueryHandler : IQueryHandler<GetAgentThreadQuery, ThreadStateDto>
{
    private readonly IBlueprintsAgentClient _client;

    public GetAgentThreadQueryHandler(IBlueprintsAgentClient client)
    {
        _client = client;
    }

    public async Task<Result<ThreadStateDto>> Handle(
        GetAgentThreadQuery request,
        CancellationToken cancellationToken)
    {
        return await _client.SafeGetThreadAsync(request.ThreadId, cancellationToken);
    }
}
