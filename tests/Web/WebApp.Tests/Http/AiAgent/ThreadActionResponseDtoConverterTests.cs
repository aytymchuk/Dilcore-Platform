using System.Text.Json;
using System.Text.Json.Serialization;
using Dilcore.WebApp.Http.AiAgent.Dtos;
using Shouldly;

namespace Dilcore.WebApp.Tests.Http.AiAgent;

[TestFixture]
public class ThreadActionResponseDtoConverterTests
{
    private static JsonSerializerOptions CreateOptions()
    {
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            PropertyNameCaseInsensitive = true
        };
        options.Converters.Add(new ThreadActionResponseDtoConverter());
        return options;
    }

    [Test]
    public void Deserialize_Should_Map_Json_With_Messages_To_ThreadContinuation()
    {
        const string json = """
            {"id":"thread-1","messages":[{"type":"human","content":"hi","agent_type":null}]}
            """;

        var result = JsonSerializer.Deserialize<ThreadActionResponseDto>(json, CreateOptions());

        result.ShouldBeOfType<ThreadContinuationResponseDto>();
        var continuation = (ThreadContinuationResponseDto)result!;
        continuation.Thread.Id.ShouldBe("thread-1");
        continuation.Thread.Messages.Count.ShouldBe(1);
        continuation.Thread.Messages[0].Type.ShouldBe("human");
        continuation.Thread.Messages[0].Content.ShouldBe("hi");
        continuation.Thread.Messages[0].AgentType.ShouldBeNull();
    }

    [Test]
    public void Deserialize_Should_Map_Json_Without_Messages_Array_To_Interrupt()
    {
        const string json = """
            {"id":"t-99","interrupts":[{"action_request":{},"config":{}}]}
            """;

        var result = JsonSerializer.Deserialize<ThreadActionResponseDto>(json, CreateOptions());

        result.ShouldBeOfType<ThreadInterruptResponseDto>();
        var interrupt = (ThreadInterruptResponseDto)result!;
        interrupt.Interrupt.Id.ShouldBe("t-99");
        interrupt.Interrupt.Interrupts.Count.ShouldBe(1);
    }

    [Test]
    public void Deserialize_Should_Treat_Empty_Messages_Array_As_Thread()
    {
        const string json = """{"id":"t","messages":[]}""";

        var result = JsonSerializer.Deserialize<ThreadActionResponseDto>(json, CreateOptions());

        result.ShouldBeOfType<ThreadContinuationResponseDto>();
        ((ThreadContinuationResponseDto)result!).Thread.Messages.ShouldBeEmpty();
    }

    [Test]
    public void Serialize_Should_Write_Thread_When_Continuation()
    {
        var dto = (ThreadActionResponseDto)new ThreadContinuationResponseDto
        {
            Thread = new ThreadStateDto
            {
                Id = "a",
                Messages = [new MessageDto { Type = "ai", Content = "ok", AgentType = "supervisor" }]
            }
        };

        var json = JsonSerializer.Serialize(dto, CreateOptions());

        json.ShouldContain("\"id\":\"a\"");
        json.ShouldContain("\"messages\"");
        json.ShouldContain("\"agent_type\":\"supervisor\"");
    }

    [Test]
    public void Serialize_Should_Write_Interrupt_When_Interrupt()
    {
        using var empty = JsonDocument.Parse("""{}""");
        var dto = (ThreadActionResponseDto)new ThreadInterruptResponseDto
        {
            Interrupt = new InterruptResponseDto
            {
                Id = "x",
                Interrupts =
                [
                    new InterruptDto
                    {
                        ActionRequest = empty.RootElement.Clone(),
                        Config = empty.RootElement.Clone()
                    }
                ]
            }
        };

        var json = JsonSerializer.Serialize(dto, CreateOptions());

        json.ShouldContain("\"id\":\"x\"");
        json.ShouldContain("\"interrupts\"");
    }

    [Test]
    public void Write_Should_Throw_For_Unknown_Derived_Type()
    {
        using var stream = new MemoryStream();
        using var writer = new Utf8JsonWriter(stream);
        Should.Throw<NotSupportedException>(() =>
            new ThreadActionResponseDtoConverter().Write(writer, new UnknownThreadAction(), CreateOptions()));
    }

    private sealed class UnknownThreadAction : ThreadActionResponseDto;
}
