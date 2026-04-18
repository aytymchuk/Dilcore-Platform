using System.Text.Json.Serialization;
using Dilcore.WebApp.Http.AiAgent.Dtos;

namespace Dilcore.WebApp.Http.AiAgent.Streaming;

/// <summary>
/// Base type for Server-Sent Events emitted by the blueprint agent stream endpoints.
/// </summary>
[JsonPolymorphic(TypeDiscriminatorPropertyName = "category")]
[JsonDerivedType(typeof(StatusStreamEvent), "status")]
[JsonDerivedType(typeof(ThinkingStreamEvent), "thinking")]
[JsonDerivedType(typeof(DeltaStreamEvent), "delta")]
[JsonDerivedType(typeof(DataStreamEvent), "data")]
[JsonDerivedType(typeof(InterruptStreamEvent), "interrupt")]
public abstract class BlueprintsAgentStreamEvent
{
    /// <summary>Optional agent discriminator; may appear on any SSE category.</summary>
    public string? AgentType { get; init; }
}

/// <summary>
/// High-level execution step (routing, design phase, etc.).
/// </summary>
public sealed class StatusStreamEvent : BlueprintsAgentStreamEvent
{
    public string? Message { get; init; }

    public string? Phase { get; init; }
}

/// <summary>
/// Model reasoning chunk when available (legacy; current API may omit this category).
/// </summary>
public sealed class ThinkingStreamEvent : BlueprintsAgentStreamEvent
{
    public string? Text { get; init; }
}

/// <summary>
/// Incremental content chunk. When <see cref="BlueprintsAgentStreamEvent.AgentType"/> is null, chunks belong to internal reasoning JSON;
/// when set, chunks are the assistant reply for that agent.
/// </summary>
public sealed class DeltaStreamEvent : BlueprintsAgentStreamEvent
{
    public string? Content { get; init; }
}

/// <summary>
/// Final thread snapshot after a stream step (thread id and full message list).
/// </summary>
public sealed class DataStreamEvent : BlueprintsAgentStreamEvent
{
    public string? ThreadId { get; init; }

    public IReadOnlyList<MessageDto>? Messages { get; init; }
}

/// <summary>
/// Graph paused for user confirmation.
/// </summary>
public sealed class InterruptStreamEvent : BlueprintsAgentStreamEvent
{
    public string? ThreadId { get; init; }

    public string? Reason { get; init; }
}
