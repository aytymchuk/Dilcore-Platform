using System.Runtime.CompilerServices;
using System.Text.Json;
using Dilcore.WebApp.Http.AiAgent.Dtos;
using Dilcore.WebApp.Http.AiAgent.Streaming;

namespace Dilcore.WebApp.Http.AiAgent;

internal sealed class BlueprintsAgentService : IBlueprintsAgentService
{
    private readonly IBlueprintsAgentClient _client;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly AgentApiSettings _settings;

    public BlueprintsAgentService(IBlueprintsAgentClient client, JsonSerializerOptions jsonOptions, AgentApiSettings settings)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _jsonOptions = jsonOptions ?? throw new ArgumentNullException(nameof(jsonOptions));
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
    }

    public async Task<ThreadActionResponseDto> StartAsync(ThreadMessageInputDto request, CancellationToken cancellationToken = default)
    {
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        cts.CancelAfter(_settings.Timeout);
        return await _client.StartAsync(request, cts.Token);
    }

    public async IAsyncEnumerable<BlueprintsAgentStreamEvent> StartStreamAsync(
        ThreadMessageInputDto request,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        using var response = await _client.StartStreamAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        await foreach (var evt in ServerSentEventsReader.ReadAsync<BlueprintsAgentStreamEvent>(stream, _jsonOptions, cancellationToken))
        {
            yield return evt;
        }
    }

    public async Task<ThreadActionResponseDto> ContinueAsync(string threadId, ThreadMessageInputDto request, CancellationToken cancellationToken = default)
    {
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        cts.CancelAfter(_settings.Timeout);
        return await _client.ContinueAsync(threadId, request, cts.Token);
    }

    public async IAsyncEnumerable<BlueprintsAgentStreamEvent> ContinueStreamAsync(
        string threadId,
        ThreadMessageInputDto request,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        using var response = await _client.ContinueStreamAsync(threadId, request, cancellationToken);
        response.EnsureSuccessStatusCode();
        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        
        await foreach (var evt in ServerSentEventsReader.ReadAsync<BlueprintsAgentStreamEvent>(stream, _jsonOptions, cancellationToken))
        {
            yield return evt;
        }
    }

    public async Task<ThreadActionResponseDto> ResumeAsync(string threadId, ThreadMessageInputDto request, CancellationToken cancellationToken = default)
    {
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        cts.CancelAfter(_settings.Timeout);
        return await _client.ResumeAsync(threadId, request, cts.Token);
    }

    public async IAsyncEnumerable<BlueprintsAgentStreamEvent> ResumeStreamAsync(
        string threadId,
        ThreadMessageInputDto request,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        using var response = await _client.ResumeStreamAsync(threadId, request, cancellationToken);
        response.EnsureSuccessStatusCode();
        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        await foreach (var evt in ServerSentEventsReader.ReadAsync<BlueprintsAgentStreamEvent>(stream, _jsonOptions, cancellationToken))
        {
            yield return evt;
        }
    }

    public async Task<IReadOnlyList<ThreadResponseDto>> GetThreadsAsync(CancellationToken cancellationToken = default)
    {
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        cts.CancelAfter(_settings.Timeout);
        return await _client.GetThreadsAsync(cts.Token);
    }

    public async Task<ThreadActionResponseDto> GetThreadAsync(string threadId, CancellationToken cancellationToken = default)
    {
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        cts.CancelAfter(_settings.Timeout);
        return await _client.GetThreadAsync(threadId, cts.Token);
    }
}
