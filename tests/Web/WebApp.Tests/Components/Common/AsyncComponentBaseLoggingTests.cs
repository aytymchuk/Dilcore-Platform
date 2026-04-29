using Bunit;
using Dilcore.WebApp.Services.Loading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Shouldly;

namespace Dilcore.WebApp.Tests.Components.Common;

public class AsyncComponentBaseLoggingTests : Bunit.TestContext
{
    [Test]
    public async Task ExecuteAsync_WhenActionThrows_LogsAndRethrows()
    {
        var logger = new Mock<ILogger<Dilcore.WebApp.Components.Common.AsyncComponentBase>>();
        logger.Setup(l => l.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
        Services.AddSingleton(logger.Object);
        Services.AddSingleton<ILoadingService>(new NoopLoadingService());

        var cut = RenderComponent<TestAsyncComponent>();

        var act = async () => await cut.Instance.RunFailingExecuteAsync();

        await Should.ThrowAsync<InvalidOperationException>(act);

        logger.Verify(l => l.Log(
                LogLevel.Error,
                It.Is<EventId>(e => e.Id == 2202),
                It.Is<It.IsAnyType>((_, __) => true),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    private sealed class NoopLoadingService : ILoadingService
    {
        public event Action? OnChange;
        public bool IsLoading => false;
        public string? CurrentMessage => null;

        public void Show(string message)
        {
            OnChange?.Invoke();
        }

        public void Hide(string message)
        {
            OnChange?.Invoke();
        }
    }

    private sealed class TestAsyncComponent : Dilcore.WebApp.Components.Common.AsyncComponentBase
    {
        public Task RunFailingExecuteAsync()
        {
            return ExecuteAsync(async () => throw new InvalidOperationException("boom"), "Test");
        }

        protected override void BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder builder)
        {
            builder.AddContent(0, "test");
        }
    }
}

