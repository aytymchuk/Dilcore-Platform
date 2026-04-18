using System.Text.Json;
using System.Text.Json.Serialization;

namespace Dilcore.WebApp.Http.AiAgent.Dtos;

/// <summary>
/// Deserializes <c>anyOf[ThreadStateDto, InterruptResponseDto]</c> from the agent API.
/// </summary>
public sealed class ThreadActionResponseDtoConverter : JsonConverter<ThreadActionResponseDto>
{
    public override ThreadActionResponseDto Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var doc = JsonDocument.ParseValue(ref reader);
        var root = doc.RootElement;

        if (root.TryGetProperty("messages", out var messages) && messages.ValueKind == JsonValueKind.Array)
        {
            var thread = root.Deserialize<ThreadStateDto>(options)
                ?? throw new JsonException("Failed to deserialize ThreadStateDto.");
            return new ThreadContinuationResponseDto { Thread = thread };
        }

        var interrupt = root.Deserialize<InterruptResponseDto>(options)
            ?? throw new JsonException("Failed to deserialize InterruptResponseDto.");
        return new ThreadInterruptResponseDto { Interrupt = interrupt };
    }

    public override void Write(Utf8JsonWriter writer, ThreadActionResponseDto value, JsonSerializerOptions options)
    {
        switch (value)
        {
            case ThreadContinuationResponseDto c:
                JsonSerializer.Serialize(writer, c.Thread, options);
                break;
            case ThreadInterruptResponseDto i:
                JsonSerializer.Serialize(writer, i.Interrupt, options);
                break;
            default:
                throw new NotSupportedException($"Unknown {nameof(ThreadActionResponseDto)} type: {value.GetType().Name}");
        }
    }
}
