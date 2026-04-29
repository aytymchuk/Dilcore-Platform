using System.Text.Json;
using Dilcore.WebApp.Http.AiAgent.Dtos;
using Dilcore.WebApp.Http.AiAgent.Streaming;
using Shouldly;

namespace Dilcore.WebApp.Tests.Http.AiAgent;

/// <summary>
/// Validates real-world SSE from the blueprint agent against <see cref="ServerSentEventsReader"/>
/// and the same buffer rules used by <c>AdminAgent</c> (thinking, delta, data finalization).
/// </summary>
[TestFixture]
public class AgentStreamScenarioTests
{
    private static JsonSerializerOptions StreamJsonOptions { get; } = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        PropertyNameCaseInsensitive = true
    };

    private static string SamplePath =>
        Path.Combine(AppContext.BaseDirectory, "Http", "AiAgent", "AgentStreamSample.sse");

    private static async Task<List<BlueprintsAgentStreamEvent>> ReadSampleAsync()
    {
        await using var stream = File.OpenRead(SamplePath);
        var list = new List<BlueprintsAgentStreamEvent>();
        await foreach (var item in ServerSentEventsReader.ReadAsync<BlueprintsAgentStreamEvent>(
                           stream,
                           StreamJsonOptions,
                           CancellationToken.None))
        {
            list.Add(item);
        }

        return list;
    }

    /// <summary>
    /// Mirrors <c>AdminAgent.SendAsync</c> stream handling (thinking, delta, status, data) without UI.
    /// </summary>
    private static void ApplyAdminAgentStreamRules(
        IReadOnlyList<BlueprintsAgentStreamEvent> events,
        out string? pendingAssistant,
        out int pendingSteps,
        out string? interruptThreadId,
        out string? lastStatus)
    {
        pendingAssistant = null;
        pendingSteps = 0;
        interruptThreadId = null;
        lastStatus = null;

        foreach (var evt in events)
        {
            switch (evt)
            {
                case DeltaStreamEvent d:
                    pendingAssistant = (pendingAssistant ?? string.Empty) + (d.Content ?? string.Empty);
                    break;
                case StatusStreamEvent s:
                    lastStatus = string.Join(
                        " — ",
                        new[] { s.Message, s.Phase }.Where(x => !string.IsNullOrWhiteSpace(x)));
                    break;
                case ThinkingStreamEvent:
                    pendingSteps++;
                    break;
                case DataStreamEvent data:
                    if (string.IsNullOrEmpty(interruptThreadId) && !string.IsNullOrEmpty(data.ThreadId))
                    {
                        interruptThreadId = data.ThreadId;
                    }

                    var lastAi = FindLastAiMessage(data.Messages);
                    if (lastAi is not null)
                    {
                        pendingAssistant = lastAi.Content;
                    }

                    break;
            }
        }
    }

    private static MessageDto? FindLastAiMessage(IReadOnlyList<MessageDto>? messages)
    {
        if (messages is null || messages.Count == 0)
        {
            return null;
        }

        for (var i = messages.Count - 1; i >= 0; i--)
        {
            var m = messages[i];
            if (string.Equals(m.Type, "ai", StringComparison.OrdinalIgnoreCase))
            {
                return m;
            }
        }

        return null;
    }

    [Test]
    public async Task RealWorldSample_Should_Deserialize_And_Match_AdminAgent_Buffer_Rules()
    {
        File.Exists(SamplePath).ShouldBeTrue($"Sample file missing at {SamplePath}");

        var events = await ReadSampleAsync();

        events.Count.ShouldBe(5);

        events[0].ShouldBeOfType<StatusStreamEvent>();
        events[1].ShouldBeOfType<ThinkingStreamEvent>();
        events[2].ShouldBeOfType<ThinkingStreamEvent>();
        events[3].ShouldBeOfType<DeltaStreamEvent>();
        events[4].ShouldBeOfType<DataStreamEvent>();

        var routingStatus = (StatusStreamEvent)events[0];
        routingStatus.Message.ShouldBe("Analyzing your request...");
        routingStatus.Phase.ShouldBe("routing");

        var data = (DataStreamEvent)events[4];
        data.ThreadId.ShouldBe("00000000-0000-0000-0000-000000000000");
        data.Messages.ShouldNotBeNull();
        data.Messages!.Count.ShouldBe(2);
        data.Messages[0].Id.ShouldBe("m-0");
        data.Messages[0].Type.ShouldBe("human");
        data.Messages[1].Id.ShouldBe("m-1");
        data.Messages[1].Type.ShouldBe("ai");
        data.Messages[1].AgentType.ShouldBe("ask");
        data.Reasoning.ShouldNotBeNull();
        data.Reasoning!.Count.ShouldBe(1);
        data.Reasoning[0].AfterMessageId.ShouldBe("m-0");

        ApplyAdminAgentStreamRules(
            events,
            out var pendingAssistant,
            out var pendingSteps,
            out var interruptThreadId,
            out var lastStatus);

        interruptThreadId.ShouldBe(data.ThreadId);
        lastStatus.ShouldBe("Analyzing your request... — routing");
        pendingSteps.ShouldBe(2);

        var lastAi = FindLastAiMessage(data.Messages);
        lastAi.ShouldNotBeNull();
        pendingAssistant.ShouldBe(lastAi!.Content);
    }
}
