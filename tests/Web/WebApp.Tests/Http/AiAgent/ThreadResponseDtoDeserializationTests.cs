using System.Text.Json;
using Dilcore.WebApp.Http.AiAgent.Dtos;
using Shouldly;

namespace Dilcore.WebApp.Tests.Http.AiAgent;

[TestFixture]
public class ThreadResponseDtoDeserializationTests
{
    private static JsonSerializerOptions CreateAgentLikeOptions() =>
        new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
            PropertyNameCaseInsensitive = true
        };

    [Test]
    public void Deserialize_Should_Map_Thread_List_Items()
    {
        const string json = """
            [{"id":"3d5d9ad4-1533-44c5-b9e7-13cd7ed5004b","name":"Help me build a data schema fo..."}]
            """;

        var list = JsonSerializer.Deserialize<IReadOnlyList<ThreadResponseDto>>(json, CreateAgentLikeOptions());

        list!.Count.ShouldBe(1);
        list[0].Id.ShouldBe("3d5d9ad4-1533-44c5-b9e7-13cd7ed5004b");
        list[0].Name.ShouldBe("Help me build a data schema fo...");
    }
}
