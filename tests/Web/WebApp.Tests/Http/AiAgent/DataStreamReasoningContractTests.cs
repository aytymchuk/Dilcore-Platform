using System.Text.Json;

using Dilcore.WebApp.Http.AiAgent.Streaming;

using FluentAssertions;

namespace Dilcore.WebApp.Tests.Http.AiAgent;

[TestFixture]
public class DataStreamReasoningContractTests
{
    private static JsonSerializerOptions StreamJsonOptions { get; } = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        PropertyNameCaseInsensitive = true
    };

    [Test]
    public void Deserialize_Data_Event_Should_Map_Reasoning_Step_Header_Content_And_Items()
    {
        const string json = """
            {
              "category": "data",
              "thread_id": "4767e4d3-62c6-480e-a4ed-1f406c035577",
              "messages": [],
              "reasoning": [
                {
                  "id": "r-73e5979f-beb7-4450-a52d-a13915da9a73",
                  "type": "reasoning",
                  "after_message_id": "m-0",
                  "sequence": 1,
                  "node": "supervisor",
                  "agent_type": null,
                  "header": "Understanding what you want to do",
                  "steps": [
                    {
                      "kind": "step",
                      "status": "completed",
                      "content": "Checking routing",
                      "items": null
                    },
                    {
                      "kind": "next_steps",
                      "status": "completed",
                      "content": null,
                      "items": ["Ask for generation when ready."]
                    }
                  ]
                }
              ]
            }
            """;

        var data = JsonSerializer.Deserialize<DataStreamEvent>(json, StreamJsonOptions);

        data.Should().NotBeNull();
        data!.ThreadId.Should().Be("4767e4d3-62c6-480e-a4ed-1f406c035577");
        data.Reasoning.Should().NotBeNull();
        data.Reasoning!.Should().HaveCount(1);

        var env = data.Reasoning[0];
        env.Id.Should().Be("r-73e5979f-beb7-4450-a52d-a13915da9a73");
        env.Header.Should().Be("Understanding what you want to do");
        env.Steps.Should().HaveCount(2);

        env.Steps![0].Kind.Should().Be("step");
        env.Steps[0].Content.Should().Be("Checking routing");
        env.Steps[0].Items.Should().BeNull();

        env.Steps[1].Kind.Should().Be("next_steps");
        env.Steps[1].Items.Should().Equal("Ask for generation when ready.");
    }

    [Test]
    public void Deserialize_Data_Event_Should_Map_Step_With_Header_And_Content_Fields()
    {
        const string json = """
            {
              "category": "data",
              "thread_id": "t-1",
              "messages": [],
              "reasoning": [
                {
                  "id": "r-1",
                  "type": "reasoning",
                  "after_message_id": "m-0",
                  "sequence": 1,
                  "steps": [
                    {
                      "kind": "summary",
                      "status": "completed",
                      "header": "Summary header",
                      "content": "Summary detail",
                      "items": null
                    }
                  ]
                }
              ]
            }
            """;

        var data = JsonSerializer.Deserialize<DataStreamEvent>(json, StreamJsonOptions);

        data.Should().NotBeNull();
        var step = data!.Reasoning![0].Steps![0];
        step.Header.Should().Be("Summary header");
        step.Content.Should().Be("Summary detail");
    }

    [Test]
    public void Deserialize_Thinking_Event_Should_Map_Header_And_Items()
    {
        const string json = """
            {
              "category": "thinking",
              "type": "reasoning",
              "header": "Step label",
              "content": "Step detail",
              "kind": "next_steps",
              "status": "completed",
              "items": ["Do this", "Then that"],
              "after_message_id": "m-0",
              "sequence": 3,
              "envelope_id": "r-9",
              "entry_index": 2,
              "node": "design",
              "agent_type": "design"
            }
            """;

        var evt = JsonSerializer.Deserialize<ThinkingStreamEvent>(json, StreamJsonOptions);

        evt.Should().NotBeNull();
        evt!.Header.Should().Be("Step label");
        evt.Content.Should().Be("Step detail");
        evt.Kind.Should().Be("next_steps");
        evt.Items.Should().Equal("Do this", "Then that");
        evt.AfterMessageId.Should().Be("m-0");
        evt.Sequence.Should().Be(3);
        evt.EnvelopeId.Should().Be("r-9");
        evt.EntryIndex.Should().Be(2);
    }
}
