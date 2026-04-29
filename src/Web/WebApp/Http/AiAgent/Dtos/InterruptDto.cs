using System.Text.Json;

namespace Dilcore.WebApp.Http.AiAgent.Dtos;

/// <summary>
/// Single pending interrupt from the agent graph API.
/// </summary>
public sealed class InterruptDto
{
    public required JsonElement ActionRequest { get; init; }

    public required JsonElement Config { get; init; }

    public string? Description { get; init; }
}
