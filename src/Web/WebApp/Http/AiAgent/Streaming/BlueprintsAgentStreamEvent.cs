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
[JsonDerivedType(typeof(ErrorStreamEvent), "error")]
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
/// Reasoning / extended-thinking fragment streamed during execution.
/// </summary>
public sealed class ThinkingStreamEvent : BlueprintsAgentStreamEvent
{
    /// <summary>Normalized block type for compatibility (<c>thinking</c> or <c>reasoning</c>).</summary>
    public string? Type { get; init; }

    /// <summary>Short UI label for this reasoning fragment when present.</summary>
    public string? Header { get; init; }

    public string? Content { get; init; }

    public string? Kind { get; init; }

    public string? Status { get; init; }

    /// <summary>Optional bullet lines when <see cref="Kind"/> is <c>next_steps</c>.</summary>
    public IReadOnlyList<string>? Items { get; init; }

    public string? AfterMessageId { get; init; }

    public int? Sequence { get; init; }

    /// <summary>Stable reasoning envelope identifier (for merging stream updates).</summary>
    public string? EnvelopeId { get; init; }

    /// <summary>Stable entry index within the envelope (for merging stream updates).</summary>
    public int? EntryIndex { get; init; }

    public string? Node { get; init; }

    public string? Phase { get; init; }
}

/// <summary>
/// Incremental assistant reply chunk.
/// </summary>
public sealed class DeltaStreamEvent : BlueprintsAgentStreamEvent
{
    public string? Content { get; init; }
}

/// <summary>
/// Terminal thread snapshot after a stream step.
/// </summary>
public sealed class DataStreamEvent : BlueprintsAgentStreamEvent
{
    public string? ThreadId { get; init; }

    public IReadOnlyList<MessageDto>? Messages { get; init; }

    public IReadOnlyList<ReasoningEnvelopeDto>? Reasoning { get; init; }
}

/// <summary>
/// Graph paused for user confirmation (SSE interrupt envelope).
/// </summary>
public sealed class InterruptStreamEvent : BlueprintsAgentStreamEvent
{
    public string? Id { get; init; }

    public IReadOnlyList<InterruptDto>? Interrupts { get; init; }

    public IReadOnlyList<MessageDto>? Messages { get; init; }

    public IReadOnlyList<ReasoningEnvelopeDto>? Reasoning { get; init; }
}

/// <summary>
/// Stream-level error payload.
/// </summary>
public sealed class ErrorStreamEvent : BlueprintsAgentStreamEvent
{
    public string? Detail { get; init; }
}
