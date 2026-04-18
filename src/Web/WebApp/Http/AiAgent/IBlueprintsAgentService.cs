using Dilcore.WebApp.Http.AiAgent.Dtos;
using Dilcore.WebApp.Http.AiAgent.Streaming;

namespace Dilcore.WebApp.Http.AiAgent;

/// <summary>
/// High-level facade for blueprint agent operations, including SSE streaming.
/// </summary>
public interface IBlueprintsAgentService
{
    Task<ThreadActionResponseDto> StartAsync(ThreadMessageInputDto request, CancellationToken cancellationToken = default);

    IAsyncEnumerable<BlueprintsAgentStreamEvent> StartStreamAsync(ThreadMessageInputDto request, CancellationToken cancellationToken = default);

    Task<ThreadActionResponseDto> ContinueAsync(string threadId, ThreadMessageInputDto request, CancellationToken cancellationToken = default);

    IAsyncEnumerable<BlueprintsAgentStreamEvent> ContinueStreamAsync(string threadId, ThreadMessageInputDto request, CancellationToken cancellationToken = default);

    Task<ThreadActionResponseDto> ResumeAsync(string threadId, ThreadMessageInputDto request, CancellationToken cancellationToken = default);

    IAsyncEnumerable<BlueprintsAgentStreamEvent> ResumeStreamAsync(string threadId, ThreadMessageInputDto request, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ThreadResponseDto>> GetThreadsAsync(CancellationToken cancellationToken = default);

    Task<ThreadResponseDto> GetThreadAsync(string threadId, CancellationToken cancellationToken = default);
}
