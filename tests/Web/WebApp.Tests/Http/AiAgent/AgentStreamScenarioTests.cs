using System.Text.Json;
using Dilcore.WebApp.Http.AiAgent.Dtos;
using Dilcore.WebApp.Http.AiAgent.Streaming;
using Shouldly;

namespace Dilcore.WebApp.Tests.Http.AiAgent;

/// <summary>
/// Validates real-world SSE from the blueprint agent against <see cref="ServerSentEventsReader"/>
/// and the same buffer rules used by <c>AdminAgent</c> (reasoning vs assistant deltas, data finalization).
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
    /// Mirrors <c>AdminAgent.SendAsync</c> stream handling (deltas, status, data) without UI.
    /// </summary>
    private static void ApplyAdminAgentStreamRules(
        IReadOnlyList<BlueprintsAgentStreamEvent> events,
        out string? pendingReasoning,
        out string? pendingAssistant,
        out string? interruptThreadId,
        out string? lastStatus)
    {
        pendingReasoning = null;
        pendingAssistant = null;
        interruptThreadId = null;
        lastStatus = null;

        foreach (var evt in events)
        {
            switch (evt)
            {
                case DeltaStreamEvent d when string.IsNullOrEmpty(d.AgentType):
                    pendingReasoning = (pendingReasoning ?? string.Empty) + (d.Content ?? string.Empty);
                    break;
                case DeltaStreamEvent d:
                    pendingAssistant = (pendingAssistant ?? string.Empty) + (d.Content ?? string.Empty);
                    break;
                case StatusStreamEvent s:
                    lastStatus = string.Join(
                        " — ",
                        new[] { s.Message, s.Phase }.Where(x => !string.IsNullOrWhiteSpace(x)));
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

        events.Count.ShouldBe(10);

        events[0].ShouldBeOfType<DeltaStreamEvent>();
        events[1].ShouldBeOfType<DeltaStreamEvent>();
        events[2].ShouldBeOfType<DeltaStreamEvent>();
        events[3].ShouldBeOfType<DeltaStreamEvent>();
        events[4].ShouldBeOfType<DeltaStreamEvent>();
        events[5].ShouldBeOfType<DeltaStreamEvent>();
        events[6].ShouldBeOfType<StatusStreamEvent>();
        events[7].ShouldBeOfType<DeltaStreamEvent>();
        events[8].ShouldBeOfType<StatusStreamEvent>();
        events[9].ShouldBeOfType<DataStreamEvent>();

        foreach (var e in events.Take(6).Cast<DeltaStreamEvent>())
        {
            string.IsNullOrEmpty(e.AgentType).ShouldBeTrue();
        }

        var designDelta = (DeltaStreamEvent)events[7];
        designDelta.AgentType.ShouldBe("design");

        var routingStatus = (StatusStreamEvent)events[6];
        routingStatus.Message.ShouldBe("Analyzing your request...");
        routingStatus.Phase.ShouldBe("routing");

        var data = (DataStreamEvent)events[9];
        data.ThreadId.ShouldBe("b68a59eb-ae38-478f-aeba-acacb3a1bff6");
        data.Messages.ShouldNotBeNull();
        data.Messages!.Count.ShouldBe(2);
        data.Messages[0].Type.ShouldBe("human");
        data.Messages[1].Type.ShouldBe("ai");
        data.Messages[1].AgentType.ShouldBe("design");

        ApplyAdminAgentStreamRules(
            events,
            out var pendingReasoning,
            out var pendingAssistant,
            out var interruptThreadId,
            out var lastStatus);

        interruptThreadId.ShouldBe(data.ThreadId);
        lastStatus.ShouldBe("Working on the design... — design");

        var expectedReasoning = string.Concat(
            Enumerable.Range(0, 6).Select(i => ((DeltaStreamEvent)events[i]).Content ?? string.Empty));
        pendingReasoning.ShouldBe(expectedReasoning);

        // JSON-shaped reasoning buffer (null agent_type deltas) should round-trip as valid JSON.
        pendingReasoning.ShouldNotBeNull();
        using var _ = JsonDocument.Parse(pendingReasoning);
        pendingReasoning.ShouldContain("next_route");
        pendingReasoning.ShouldContain("design");

        var lastAi = FindLastAiMessage(data.Messages);
        lastAi.ShouldNotBeNull();
        pendingAssistant.ShouldBe(lastAi!.Content);
        pendingAssistant.ShouldBe(designDelta.Content);
    }
}
