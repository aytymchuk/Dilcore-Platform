using System.Text.Json.Serialization;

namespace Dilcore.WebApp.Http.AiAgent.Dtos;

/// <summary>
/// Union of <see cref="ThreadResponseDto"/> and <see cref="InterruptResponseDto"/> returned by non-streaming agent actions.
/// </summary>
[JsonConverter(typeof(ThreadActionResponseDtoConverter))]
public abstract class ThreadActionResponseDto
{
    /// <summary>
    /// Gets the response as a normal thread continuation, if applicable.
    /// </summary>
    public ThreadResponseDto? AsThread() => this is ThreadContinuationResponseDto c ? c.Thread : null;

    /// <summary>
    /// Gets the response as an interrupt, if applicable.
    /// </summary>
    public InterruptResponseDto? AsInterrupt() => this is ThreadInterruptResponseDto i ? i.Interrupt : null;
}

/// <summary>
/// Thread state after a successful step.
/// </summary>
public sealed class ThreadContinuationResponseDto : ThreadActionResponseDto
{
    /// <summary>
    /// Thread payload.
    /// </summary>
    public required ThreadResponseDto Thread { get; init; }
}

/// <summary>
/// Execution paused for user input.
/// </summary>
public sealed class ThreadInterruptResponseDto : ThreadActionResponseDto
{
    /// <summary>
    /// Interrupt payload.
    /// </summary>
    public required InterruptResponseDto Interrupt { get; init; }
}
