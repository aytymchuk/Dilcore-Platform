using Dilcore.WebApp.Http.AiAgent.Dtos;
using Dilcore.WebApp.Models.Agent;

namespace Dilcore.WebApp.Services.Agent;

/// <summary>
/// Maps agent thread DTOs into UI <see cref="ChatMessage"/> lists with anchored reasoning.
/// </summary>
public static class AgentThreadMessagesMapper
{
    public static List<ChatMessage> Map(ThreadStateDto thread)
    {
        var ordered = thread.Messages;
        var chatMessages = new List<ChatMessage>();
        var idToChatIndex = new Dictionary<string, int>(StringComparer.Ordinal);

        for (var i = 0; i < ordered.Count; i++)
        {
            var m = ordered[i];
            if (IsToolMessageType(m.Type))
            {
                continue;
            }

            var author = IsUserMessageType(m.Type)
                ? ChatAuthor.User
                : ChatAuthor.Assistant;
            var ts = DateTime.UtcNow.AddSeconds(-(ordered.Count - i));
            var cm = new ChatMessage(Guid.NewGuid(), author, m.Content, ts)
            {
                ApiMessageId = m.Id
            };

            var idx = chatMessages.Count;
            chatMessages.Add(cm);

            if (!string.IsNullOrEmpty(m.Id))
            {
                idToChatIndex[m.Id] = idx;
            }
        }

        AttachReasoning(chatMessages, ordered, thread.Reasoning, idToChatIndex);

        return chatMessages;
    }

    private static void AttachReasoning(
        List<ChatMessage> chatMessages,
        IReadOnlyList<MessageDto> orderedFull,
        IReadOnlyList<ReasoningEnvelopeDto> reasoning,
        Dictionary<string, int> idToChatIndex)
    {
        if (reasoning.Count == 0)
        {
            return;
        }

        foreach (var env in reasoning.OrderBy(r => r.Sequence))
        {
            var chatIdx = ResolveChatIndexForAnchor(orderedFull, idToChatIndex, env.AfterMessageId);
            if (chatIdx is null)
            {
                continue;
            }

            var addition = AgentReasoningViewMapper.FromEnvelopes([env]);
            var existing = chatMessages[chatIdx.Value].Reasoning;
            chatMessages[chatIdx.Value] = chatMessages[chatIdx.Value] with
            {
                Reasoning = MergeReasoning(existing, addition)
            };
        }
    }

    private static int? ResolveChatIndexForAnchor(
        IReadOnlyList<MessageDto> orderedFull,
        Dictionary<string, int> idToChatIndex,
        string afterMessageId)
    {
        var anchorIdx = IndexOfMessageId(orderedFull, afterMessageId);
        if (anchorIdx < 0)
        {
            return null;
        }

        for (var i = anchorIdx; i < orderedFull.Count; i++)
        {
            var m = orderedFull[i];
            if (IsToolMessageType(m.Type))
            {
                continue;
            }

            if (string.IsNullOrEmpty(m.Id))
            {
                continue;
            }

            if (idToChatIndex.TryGetValue(m.Id, out var chatIdx))
            {
                return chatIdx;
            }
        }

        return null;
    }

    private static int IndexOfMessageId(IReadOnlyList<MessageDto> orderedFull, string id)
    {
        for (var i = 0; i < orderedFull.Count; i++)
        {
            if (string.Equals(orderedFull[i].Id, id, StringComparison.Ordinal))
            {
                return i;
            }
        }

        return -1;
    }

    private static AgentReasoningProcess MergeReasoning(AgentReasoningProcess? existing, AgentReasoningProcess addition)
    {
        if (existing is null)
        {
            return addition;
        }

        var mergedSections = new List<AgentReasoningSection>();
        var indexByEnvelopeId = new Dictionary<string, int>(StringComparer.Ordinal);

        foreach (var section in existing.Sections)
        {
            var idx = mergedSections.Count;
            mergedSections.Add(section);

            if (!string.IsNullOrEmpty(section.EnvelopeId))
            {
                indexByEnvelopeId[section.EnvelopeId] = idx;
            }
        }

        foreach (var section in addition.Sections)
        {
            if (!string.IsNullOrEmpty(section.EnvelopeId) && indexByEnvelopeId.TryGetValue(section.EnvelopeId, out var existingIdx))
            {
                mergedSections[existingIdx] = section;
                continue;
            }

            var idx = mergedSections.Count;
            mergedSections.Add(section);
            if (!string.IsNullOrEmpty(section.EnvelopeId))
            {
                indexByEnvelopeId[section.EnvelopeId] = idx;
            }
        }

        return new AgentReasoningProcess
        {
            Title = existing.Title,
            Subtitle = existing.Subtitle,
            TotalDurationText = existing.TotalDurationText,
            CanCopyLogs = existing.CanCopyLogs,
            CopyLogsText = existing.CopyLogsText,
            Sections = mergedSections
        };
    }

    private static bool IsUserMessageType(string type) =>
        string.Equals(type, "user", StringComparison.OrdinalIgnoreCase)
        || string.Equals(type, "human", StringComparison.OrdinalIgnoreCase);

    private static bool IsToolMessageType(string type) =>
        string.Equals(type, "tool", StringComparison.OrdinalIgnoreCase);
}
