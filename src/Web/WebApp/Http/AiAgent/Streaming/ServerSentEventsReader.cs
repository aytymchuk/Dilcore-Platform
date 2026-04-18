using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;

namespace Dilcore.WebApp.Http.AiAgent.Streaming;

/// <summary>
/// Parses <c>text/event-stream</c> bodies and yields deserialized JSON payloads from <c>data:</c> lines.
/// </summary>
public static class ServerSentEventsReader
{
    /// <summary>
    /// Reads SSE events from a stream and deserializes each complete event as <typeparamref name="T"/>.
    /// </summary>
    public static async IAsyncEnumerable<T> ReadAsync<T>(
        Stream stream,
        JsonSerializerOptions options,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        using var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: false, bufferSize: 1024, leaveOpen: true);
        var dataBuffer = new StringBuilder();

        while (true)
        {
            var line = await reader.ReadLineAsync(cancellationToken).ConfigureAwait(false);
            if (line is null)
            {
                if (dataBuffer.Length > 0)
                {
                    foreach (var item in FlushBuffer<T>(dataBuffer, options))
                    {
                        yield return item;
                    }
                }

                yield break;
            }

            if (line.Length == 0)
            {
                foreach (var item in FlushBuffer<T>(dataBuffer, options))
                {
                    yield return item;
                }

                continue;
            }

            if (line.StartsWith(":", StringComparison.Ordinal))
            {
                continue;
            }

            if (line.StartsWith("data:", StringComparison.Ordinal))
            {
                var data = line.AsSpan(5).TrimStart();
                if (dataBuffer.Length > 0)
                {
                    dataBuffer.Append('\n');
                }

                dataBuffer.Append(data);
            }
        }
    }

    private static IEnumerable<T> FlushBuffer<T>(StringBuilder dataBuffer, JsonSerializerOptions options)
    {
        if (dataBuffer.Length == 0)
        {
            yield break;
        }

        var payload = dataBuffer.ToString();
        dataBuffer.Clear();

        if (string.Equals(payload.Trim(), "[DONE]", StringComparison.OrdinalIgnoreCase))
        {
            yield break;
        }

        var deserialized = JsonSerializer.Deserialize<T>(payload, options)
            ?? throw new JsonException("SSE data line deserialized to null.");

        yield return deserialized;
    }
}
