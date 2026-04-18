using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Dilcore.WebApp.Http.AiAgent;
using Dilcore.WebApp.Http.AiAgent.Dtos;
using Dilcore.WebApp.Http.AiAgent.Streaming;
using Moq;
using Shouldly;

namespace Dilcore.WebApp.Tests.Http.AiAgent;

[TestFixture]
public class BlueprintsAgentServiceTests
{
    private static JsonSerializerOptions CreateJsonOptions()
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

    private static AgentApiSettings CreateSettings() => new()
    {
        BaseUrl = new Uri("http://localhost"),
        Timeout = TimeSpan.FromSeconds(30)
    };

    [Test]
    public async Task StartAsync_Should_Return_Client_Result()
    {
        var expected = new ThreadContinuationResponseDto
        {
            Thread = new ThreadStateDto { Id = "t1", Messages = [] }
        };
        var client = new Mock<IBlueprintsAgentClient>();
        client
            .Setup(c => c.StartAsync(It.IsAny<ThreadMessageInputDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ThreadActionResponseDto)expected);

        var sut = new BlueprintsAgentService(client.Object, CreateJsonOptions(), CreateSettings());
        var request = new ThreadMessageInputDto { Message = "hi" };

        var result = await sut.StartAsync(request, CancellationToken.None);

        result.ShouldBeSameAs(expected);
        client.Verify(c => c.StartAsync(request, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task StartStreamAsync_Should_Parse_Sse_From_Response_Stream()
    {
        const string sse = """
            data: {"category":"delta","content":"chunk"}

            """;
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new ByteArrayContent(Encoding.UTF8.GetBytes(sse))
        };
        response.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("text/event-stream");

        var client = new Mock<IBlueprintsAgentClient>();
        client
            .Setup(c => c.StartStreamAsync(It.IsAny<ThreadMessageInputDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        var sut = new BlueprintsAgentService(client.Object, CreateJsonOptions(), CreateSettings());
        var request = new ThreadMessageInputDto { Message = "go" };

        var events = new List<BlueprintsAgentStreamEvent>();
        await foreach (var e in sut.StartStreamAsync(request, CancellationToken.None))
        {
            events.Add(e);
        }

        events.Count.ShouldBe(1);
        events[0].ShouldBeOfType<DeltaStreamEvent>();
        ((DeltaStreamEvent)events[0]).Content.ShouldBe("chunk");
    }

    [Test]
    public async Task StartStreamAsync_Should_Parse_Data_Event_With_Messages()
    {
        const string sse = """
            data: {"category":"data","thread_id":"t-data","messages":[{"type":"ai","content":"final"}]}

            """;
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new ByteArrayContent(Encoding.UTF8.GetBytes(sse))
        };
        response.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("text/event-stream");

        var client = new Mock<IBlueprintsAgentClient>();
        client
            .Setup(c => c.StartStreamAsync(It.IsAny<ThreadMessageInputDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        var sut = new BlueprintsAgentService(client.Object, CreateJsonOptions(), CreateSettings());
        var request = new ThreadMessageInputDto { Message = "go" };

        var events = new List<BlueprintsAgentStreamEvent>();
        await foreach (var e in sut.StartStreamAsync(request, CancellationToken.None))
        {
            events.Add(e);
        }

        events.Count.ShouldBe(1);
        events[0].ShouldBeOfType<DataStreamEvent>();
        var data = (DataStreamEvent)events[0];
        data.ThreadId.ShouldBe("t-data");
        data.Messages.ShouldNotBeNull();
        data.Messages!.Count.ShouldBe(1);
        data.Messages[0].Type.ShouldBe("ai");
        data.Messages[0].Content.ShouldBe("final");
    }

    [Test]
    public async Task StartStreamAsync_Should_Throw_When_Status_Is_Not_Success()
    {
        var response = new HttpResponseMessage(HttpStatusCode.BadRequest)
        {
            Content = new StringContent("error")
        };

        var client = new Mock<IBlueprintsAgentClient>();
        client
            .Setup(c => c.StartStreamAsync(It.IsAny<ThreadMessageInputDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        var sut = new BlueprintsAgentService(client.Object, CreateJsonOptions(), CreateSettings());

        await Should.ThrowAsync<HttpRequestException>(async () =>
        {
            await foreach (var _ in sut.StartStreamAsync(new ThreadMessageInputDto { Message = "x" }, CancellationToken.None))
            {
            }
        });
    }

    [Test]
    public async Task GetThreadAsync_Should_Return_Client_Result()
    {
        var expected = new ThreadStateDto { Id = "t2", Messages = [] };
        var client = new Mock<IBlueprintsAgentClient>();
        client
            .Setup(c => c.GetThreadAsync("t2", It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var sut = new BlueprintsAgentService(client.Object, CreateJsonOptions(), CreateSettings());

        var result = await sut.GetThreadAsync("t2", CancellationToken.None);

        result.ShouldBeSameAs(expected);
    }
}
