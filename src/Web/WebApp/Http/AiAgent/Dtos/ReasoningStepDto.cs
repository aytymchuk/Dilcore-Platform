namespace Dilcore.WebApp.Http.AiAgent.Dtos;

/// <summary>
/// A step row inside a reasoning envelope (<see cref="ReasoningEnvelopeDto"/>).
/// </summary>
public sealed class ReasoningStepDto
{
    public string? Kind { get; init; }

    public string? Status { get; init; }

    /// <summary>Short UI label when present; optional detail in <see cref="Content"/>.</summary>
    public string? Header { get; init; }

    public string? Content { get; init; }

    /// <summary>Optional bullet lines for <c>next_steps</c> rows.</summary>
    public IReadOnlyList<string>? Items { get; init; }
}
