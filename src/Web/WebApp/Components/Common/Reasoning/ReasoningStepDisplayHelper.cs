using Dilcore.WebApp.Models.Agent;

namespace Dilcore.WebApp.Components.Common.Reasoning;

/// <summary>Shared rules for reasoning row titles and summary detection.</summary>
public static class ReasoningStepDisplayHelper
{
    public const int DefaultTitleTruncationLength = 30;

    public static bool IsSummaryKind(AgentReasoningStep step)
    {
        return string.Equals(step.Kind, "summary", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>Short panel/header title when <see cref="AgentReasoningStep.Header"/> is absent.</summary>
    public static string TruncateForStepTitle(string? content, int maxLength = DefaultTitleTruncationLength)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            return string.Empty;
        }

        var trimmed = content.Trim();
        if (trimmed.Length <= maxLength)
        {
            return trimmed;
        }

        var slice = trimmed[..maxLength].TrimEnd();
        return $"{slice}...";
    }
}
