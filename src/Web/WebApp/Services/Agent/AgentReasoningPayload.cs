using System.Text.Json;

namespace Dilcore.WebApp.Services.Agent;

/// <summary>
/// Parses the model's reasoning buffer when it is JSON with <c>reasoning</c> and optional <c>decision.next_route</c>.
/// </summary>
public static class AgentReasoningPayload
{
    /// <summary>
    /// Attempts to parse structured reasoning. When <see cref="IsStructured"/> is false, show <see cref="RawText"/> as a fallback (e.g. streaming partial JSON).
    /// </summary>
    public static AgentReasoningDisplay Parse(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return new AgentReasoningDisplay(false, null, null, string.Empty);
        }

        var trimmed = text.Trim();
        try
        {
            using var doc = JsonDocument.Parse(trimmed);
            var root = doc.RootElement;
            if (root.ValueKind != JsonValueKind.Object)
            {
                return new AgentReasoningDisplay(false, null, null, text);
            }

            string? narrative = null;
            if (root.TryGetProperty("reasoning", out var reasoningEl)
                && reasoningEl.ValueKind == JsonValueKind.String)
            {
                narrative = reasoningEl.GetString();
            }

            string? nextRoute = null;
            if (root.TryGetProperty("decision", out var decisionEl)
                && decisionEl.ValueKind == JsonValueKind.Object
                && decisionEl.TryGetProperty("next_route", out var routeEl))
            {
                nextRoute = routeEl.ValueKind switch
                {
                    JsonValueKind.String => routeEl.GetString(),
                    JsonValueKind.Number => routeEl.GetRawText(),
                    _ => null
                };
            }

            if (narrative is not null || !string.IsNullOrEmpty(nextRoute))
            {
                return new AgentReasoningDisplay(true, narrative, nextRoute, text);
            }
        }
        catch (JsonException)
        {
            // Incomplete stream or non-JSON reasoning text.
        }

        return new AgentReasoningDisplay(false, null, null, text);
    }
}

/// <param name="IsStructured">True when JSON contained at least reasoning or decision.</param>
public readonly record struct AgentReasoningDisplay(
    bool IsStructured,
    string? Narrative,
    string? NextRoute,
    string RawText);
