using Dilcore.WebApp.Http.AiAgent.Dtos;

namespace Dilcore.WebApp.Services.Agent;

internal sealed class ConversationTitleFactory : IConversationTitleFactory
{
    private const int MaxTitleLength = 50;

    public string BuildTitle(ThreadResponseDto thread)
    {
        if (!string.IsNullOrWhiteSpace(thread.Name))
        {
            return TruncateTitleLine(thread.Name);
        }

        return ShortThreadLabel(thread.Id);
    }

    public string BuildTitle(ThreadStateDto thread)
    {
        if (!string.IsNullOrWhiteSpace(thread.Name))
        {
            return TruncateTitleLine(thread.Name);
        }

        var firstUser = thread.Messages.FirstOrDefault(m =>
            string.Equals(m.Type, "user", StringComparison.OrdinalIgnoreCase));

        if (firstUser is { Content: var content } && !string.IsNullOrWhiteSpace(content))
        {
            return TruncateTitleLine(content);
        }

        return ShortThreadLabel(thread.Id);
    }

    private static string TruncateTitleLine(string text)
    {
        var singleLine = text.Trim().ReplaceLineEndings(" ");
        return singleLine.Length <= MaxTitleLength
            ? singleLine
            : string.Concat(singleLine.AsSpan(0, MaxTitleLength), "…");
    }

    private static string ShortThreadLabel(string id)
    {
        var shortId = id.Length >= 8 ? id[..8] : id;
        return $"Thread {shortId}";
    }
}
