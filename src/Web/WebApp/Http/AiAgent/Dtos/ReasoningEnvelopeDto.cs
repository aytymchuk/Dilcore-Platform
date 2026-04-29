namespace Dilcore.WebApp.Http.AiAgent.Dtos;

/// <summary>
/// Persisted reasoning envelope anchored after a thread message.
/// </summary>
public sealed class ReasoningEnvelopeDto
{
    public required string Id { get; init; }

    public required string Type { get; init; }

    public required string AfterMessageId { get; init; }

    public required int Sequence { get; init; }

    public string? Node { get; init; }

    public string? AgentType { get; init; }

    public string? Header { get; init; }

    public IReadOnlyList<ReasoningStepDto>? Steps { get; init; }
}
