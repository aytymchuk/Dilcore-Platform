namespace Dilcore.WebApp.Http.AiAgent.Dtos;

/// <summary>
/// Thread summary row returned by <c>GET /api/v1/blueprints/threads</c> (identifier and display name only).
/// </summary>
public sealed class ThreadResponseDto
{
    /// <summary>
    /// Unique thread identifier.
    /// </summary>
    public required string Id { get; init; }

    /// <summary>
    /// Display name for the thread.
    /// </summary>
    public string? Name { get; init; }
}
