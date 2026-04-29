using System.Net;
using Dilcore.WebApp.Features.Tenants.Admin.Agent.Queries;
using Dilcore.WebApp.Http.AiAgent;
using Dilcore.WebApp.Http.AiAgent.Dtos;
using Moq;
using Shouldly;
using Refit;

namespace Dilcore.WebApp.Tests.Features.Tenants.Admin.Agent.Queries;

[TestFixture]
public class AgentQueriesHandlerTests
{
    private static AgentApiSettings BuildSettings()
    {
        return new AgentApiSettings
        {
            BaseUrl = new Uri("http://localhost:8000"),
            Timeout = TimeSpan.FromSeconds(5)
        };
    }

    [Test]
    public async Task GetAgentThreadsQueryHandler_Should_Delegate_To_SafeGetThreads()
    {
        var expected = new List<ThreadResponseDto> { new() { Id = "a", Name = "N" } };
        var client = new Mock<IBlueprintsAgentClient>();
        client
            .Setup(c => c.GetThreadsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var sut = new GetAgentThreadsQueryHandler(client.Object, BuildSettings());
        var result = await sut.Handle(new GetAgentThreadsQuery(), CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBeSameAs(expected);
    }

    [Test]
    public async Task GetAgentThreadsQueryHandler_Should_Propagate_Failure_From_Client()
    {
        var client = new Mock<IBlueprintsAgentClient>();
        var apiException = await ApiException.Create(
            new HttpRequestMessage(),
            HttpMethod.Get,
            new HttpResponseMessage(HttpStatusCode.BadGateway) { Content = new StringContent("") },
            new RefitSettings());
        client
            .Setup(c => c.GetThreadsAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(apiException);

        var sut = new GetAgentThreadsQueryHandler(client.Object, BuildSettings());
        var result = await sut.Handle(new GetAgentThreadsQuery(), CancellationToken.None);

        result.IsFailed.ShouldBeTrue();
    }

    [Test]
    public async Task GetAgentThreadQueryHandler_Should_Delegate_To_SafeGetThread()
    {
        var expected = (ThreadActionResponseDto)new ThreadContinuationResponseDto
        {
            Thread = new ThreadStateDto { Id = "tid", Messages = [] }
        };
        var client = new Mock<IBlueprintsAgentClient>();
        client
            .Setup(c => c.GetThreadAsync("tid", It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var sut = new GetAgentThreadQueryHandler(client.Object, BuildSettings());
        var result = await sut.Handle(new GetAgentThreadQuery("tid"), CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBeSameAs(expected);
    }

    [Test]
    public async Task GetAgentThreadQueryHandler_Should_Propagate_Failure_From_Client()
    {
        var client = new Mock<IBlueprintsAgentClient>();
        var apiException = await ApiException.Create(
            new HttpRequestMessage(),
            HttpMethod.Get,
            new HttpResponseMessage(HttpStatusCode.NotFound) { Content = new StringContent("") },
            new RefitSettings());
        client
            .Setup(c => c.GetThreadAsync("x", It.IsAny<CancellationToken>()))
            .ThrowsAsync(apiException);

        var sut = new GetAgentThreadQueryHandler(client.Object, BuildSettings());
        var result = await sut.Handle(new GetAgentThreadQuery("x"), CancellationToken.None);

        result.IsFailed.ShouldBeTrue();
    }
}
