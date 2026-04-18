using System.Text;
using System.Text.Json;
using Dilcore.WebApp.Http.AiAgent.Streaming;
using Shouldly;

namespace Dilcore.WebApp.Tests.Http.AiAgent;

[TestFixture]
public class ServerSentEventsReaderTests
{
    private static JsonSerializerOptions StreamJsonOptions { get; } = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        PropertyNameCaseInsensitive = true
    };

    private static async Task<List<T>> CollectAsync<T>(string sse)
    {
        await using var stream = new MemoryStream(Encoding.UTF8.GetBytes(sse));
        var list = new List<T>();
        await foreach (var item in ServerSentEventsReader.ReadAsync<T>(stream, StreamJsonOptions, CancellationToken.None))
        {
            list.Add(item);
        }

        return list;
    }

    [Test]
    public async Task ReadAsync_Should_Yield_One_Event_Per_Sse_Message()
    {
        const string sse = """
            data: {"category":"delta","text":"a"}

            """;

        var events = await CollectAsync<BlueprintsAgentStreamEvent>(sse);

        events.Count.ShouldBe(1);
        events[0].ShouldBeOfType<DeltaStreamEvent>();
        ((DeltaStreamEvent)events[0]).Text.ShouldBe("a");
    }

    [Test]
    public async Task ReadAsync_Should_Join_Multiple_Data_Lines_With_Newline_Inside_Payload()
    {
        // Per SSE, multiple `data:` lines for one event are joined with '\n'. Whitespace (including newlines) is allowed between JSON tokens.
        const string sse = """
            data: {
            data: "category":"delta","text":"joined"
            data: }

            """;

        var events = await CollectAsync<BlueprintsAgentStreamEvent>(sse);

        events.Count.ShouldBe(1);
        events[0].ShouldBeOfType<DeltaStreamEvent>();
        ((DeltaStreamEvent)events[0]).Text.ShouldBe("joined");
    }

    [Test]
    public async Task ReadAsync_Should_Ignore_Non_Data_Field_Lines()
    {
        const string sse = """
            event: message
            id: 1
            data: {"category":"delta","text":"x"}

            """;

        var events = await CollectAsync<BlueprintsAgentStreamEvent>(sse);

        events.Count.ShouldBe(1);
        ((DeltaStreamEvent)events[0]).Text.ShouldBe("x");
    }

    [Test]
    public async Task ReadAsync_Should_Emit_Multiple_Events_Separated_By_Blank_Line()
    {
        const string sse = """
            data: {"category":"status","step":"one"}

            data: {"category":"status","step":"two"}

            """;

        var events = await CollectAsync<BlueprintsAgentStreamEvent>(sse);

        events.Count.ShouldBe(2);
        ((StatusStreamEvent)events[0]).Step.ShouldBe("one");
        ((StatusStreamEvent)events[1]).Step.ShouldBe("two");
    }

    [Test]
    public async Task ReadAsync_Should_Skip_Comment_Lines()
    {
        const string sse = """
            : ping
            data: {"category":"interrupt","thread_id":"t1"}

            """;

        var events = await CollectAsync<BlueprintsAgentStreamEvent>(sse);

        events.Count.ShouldBe(1);
        events[0].ShouldBeOfType<InterruptStreamEvent>();
        ((InterruptStreamEvent)events[0]).ThreadId.ShouldBe("t1");
    }

    [Test]
    public async Task ReadAsync_Should_Not_Yield_When_Data_Is_Done_Sentinel()
    {
        const string sse = """
            data: [DONE]

            """;

        var events = await CollectAsync<BlueprintsAgentStreamEvent>(sse);

        events.ShouldBeEmpty();
    }

    [Test]
    public async Task ReadAsync_Should_Flush_Last_Event_When_Stream_Ends_Without_Trailing_Blank_Line()
    {
        const string sse = """data: {"category":"delta","text":"end"}""";

        var events = await CollectAsync<BlueprintsAgentStreamEvent>(sse);

        events.Count.ShouldBe(1);
        ((DeltaStreamEvent)events[0]).Text.ShouldBe("end");
    }
}
