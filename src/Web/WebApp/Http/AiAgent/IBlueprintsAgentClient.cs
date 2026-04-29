using Dilcore.WebApp.Http.AiAgent.Dtos;
using Refit;

namespace Dilcore.WebApp.Http.AiAgent;

/// <summary>
/// Refit client for the blueprint agent (LangGraph) HTTP API.
/// </summary>
public interface IBlueprintsAgentClient
{
    [Post("/api/v1/blueprints/start")]
    Task<ThreadActionResponseDto> StartAsync([Body] ThreadMessageInputDto request, CancellationToken cancellationToken = default);

    [Headers("Accept: text/event-stream")]
    [Post("/api/v1/blueprints/start-stream")]
    Task<HttpResponseMessage> StartStreamAsync([Body] ThreadMessageInputDto request, CancellationToken cancellationToken = default);

    [Post("/api/v1/blueprints/{threadId}/continue")]
    Task<ThreadActionResponseDto> ContinueAsync(string threadId, [Body] ThreadMessageInputDto request, CancellationToken cancellationToken = default);

    [Headers("Accept: text/event-stream")]
    [Post("/api/v1/blueprints/{threadId}/continue-stream")]
    Task<HttpResponseMessage> ContinueStreamAsync(string threadId, [Body] ThreadMessageInputDto request, CancellationToken cancellationToken = default);

    [Post("/api/v1/blueprints/{threadId}/resume")]
    Task<ThreadActionResponseDto> ResumeAsync(string threadId, [Body] ThreadMessageInputDto request, CancellationToken cancellationToken = default);

    [Headers("Accept: text/event-stream")]
    [Post("/api/v1/blueprints/{threadId}/resume-stream")]
    Task<HttpResponseMessage> ResumeStreamAsync(string threadId, [Body] ThreadMessageInputDto request, CancellationToken cancellationToken = default);

    [Get("/api/v1/blueprints/threads")]
    Task<IReadOnlyList<ThreadResponseDto>> GetThreadsAsync(CancellationToken cancellationToken = default);

    [Get("/api/v1/blueprints/threads/{threadId}")]
    Task<ThreadActionResponseDto> GetThreadAsync(string threadId, CancellationToken cancellationToken = default);
}
