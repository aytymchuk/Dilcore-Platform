using System.Net;
using Dilcore.WebApp.Http.AiAgent;
using Dilcore.WebApp.Http.AiAgent.Dtos;
using Dilcore.WebApp.Http.AiAgent.Extensions;
using Moq;
using Shouldly;
using Refit;

namespace Dilcore.WebApp.Tests.Http.AiAgent;

[TestFixture]
public class BlueprintsAgentClientExtensionsTests
{
    [Test]
    public async Task SafeGetThreadsAsync_Should_Return_Value_When_Client_Succeeds()
    {
        var expected = new List<ThreadResponseDto>
        {
            new() { Id = "t1", Name = "T1" }
        };
        var client = new Mock<IBlueprintsAgentClient>();
        client
            .Setup(c => c.GetThreadsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var result = await client.Object.SafeGetThreadsAsync(CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBeSameAs(expected);
    }

    [Test]
    public async Task SafeGetThreadsAsync_Should_Return_Failed_When_ApiException()
    {
        var client = new Mock<IBlueprintsAgentClient>();
        var apiException = await ApiException.Create(
            new HttpRequestMessage(),
            HttpMethod.Get,
            new HttpResponseMessage(HttpStatusCode.InternalServerError) { Content = new StringContent("") },
            new RefitSettings());
        client
            .Setup(c => c.GetThreadsAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(apiException);

        var result = await client.Object.SafeGetThreadsAsync(CancellationToken.None);

        result.IsFailed.ShouldBeTrue();
    }

    [Test]
    public async Task SafeGetThreadAsync_Should_Return_Value_When_Client_Succeeds()
    {
        var expected = (ThreadActionResponseDto)new ThreadContinuationResponseDto
        {
            Thread = new ThreadStateDto { Id = "t1", Messages = [] }
        };
        var client = new Mock<IBlueprintsAgentClient>();
        client
            .Setup(c => c.GetThreadAsync("t1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var result = await client.Object.SafeGetThreadAsync("t1", CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBeSameAs(expected);
    }

    [Test]
    public async Task SafeGetThreadAsync_Should_Return_Failed_When_NotFound()
    {
        var client = new Mock<IBlueprintsAgentClient>();
        var apiException = await ApiException.Create(
            new HttpRequestMessage(),
            HttpMethod.Get,
            new HttpResponseMessage(HttpStatusCode.NotFound) { Content = new StringContent("") },
            new RefitSettings());
        client
            .Setup(c => c.GetThreadAsync("missing", It.IsAny<CancellationToken>()))
            .ThrowsAsync(apiException);

        var result = await client.Object.SafeGetThreadAsync("missing", CancellationToken.None);

        result.IsFailed.ShouldBeTrue();
    }
}
