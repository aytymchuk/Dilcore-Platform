using Dilcore.WebApp.Http.AiAgent.Dtos;
using FluentResults;

namespace Dilcore.WebApp.Http.AiAgent.Extensions;

/// <summary>
/// Result-based wrappers for blueprint agent Refit calls (non-streaming).
/// </summary>
public static class BlueprintsAgentClientExtensions
{
    public static Task<Result<IReadOnlyList<ThreadResponseDto>>> SafeGetThreadsAsync(
        this IBlueprintsAgentClient client,
        CancellationToken cancellationToken = default)
    {
        return SafeAgentApiInvoker.InvokeAsync(() => client.GetThreadsAsync(cancellationToken));
    }

    public static Task<Result<ThreadStateDto>> SafeGetThreadAsync(
        this IBlueprintsAgentClient client,
        string threadId,
        CancellationToken cancellationToken = default)
    {
        return SafeAgentApiInvoker.InvokeAsync(() => client.GetThreadAsync(threadId, cancellationToken));
    }
}
