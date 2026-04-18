using System.Text.Json.Serialization;

namespace Dilcore.WebApp.Http.AiAgent.Streaming;

/// <summary>
/// Base type for Server-Sent Events emitted by the blueprint agent stream endpoints.
/// </summary>
[JsonPolymorphic(TypeDiscriminatorPropertyName = "category")]
[JsonDerivedType(typeof(StatusStreamEvent), "status")]
[JsonDerivedType(typeof(ThinkingStreamEvent), "thinking")]
[JsonDerivedType(typeof(DeltaStreamEvent), "delta")]
[JsonDerivedType(typeof(InterruptStreamEvent), "interrupt")]
public abstract class BlueprintsAgentStreamEvent
{
}

/// <summary>
/// High-level execution step.
/// </summary>
public sealed class StatusStreamEvent : BlueprintsAgentStreamEvent
{
    public string? Step { get; init; }

    public string? Detail { get; init; }
}

/// <summary>
/// Model reasoning chunk when available.
/// </summary>
public sealed class ThinkingStreamEvent : BlueprintsAgentStreamEvent
{
    public string? Text { get; init; }
}

/// <summary>
/// Incremental assistant content.
/// </summary>
public sealed class DeltaStreamEvent : BlueprintsAgentStreamEvent
{
    public string? Text { get; init; }
}

/// <summary>
/// Graph paused for user confirmation.
/// </summary>
public sealed class InterruptStreamEvent : BlueprintsAgentStreamEvent
{
    public string? ThreadId { get; init; }

    public string? Reason { get; init; }
}
