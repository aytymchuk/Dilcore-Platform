using FluentResults;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using Shouldly;

namespace Dilcore.WebApp.Tests.Behaviors;

public class OperationLoggingBehaviorTests
{
    private static Mock<ILogger<T>> CreateLogger<T>()
    {
        var logger = new Mock<ILogger<T>>();
        logger.Setup(l => l.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
        return logger;
    }

    private static void VerifyEventLogged<T>(Mock<ILogger<T>> logger, int eventId, LogLevel level, Times times)
    {
        logger.Verify(l => l.Log(
                level,
                It.Is<EventId>(e => e.Id == eventId),
                It.Is<It.IsAnyType>((_, __) => true),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            times);
    }

    public record TestQuery : Dilcore.MediatR.Abstractions.IQuery<string>;
    public record TestCommand : Dilcore.MediatR.Abstractions.ICommand<string>;

    [Test]
    public async Task Handle_QuerySuccess_LogsStartAndSuccess()
    {
        var logger = CreateLogger<Dilcore.WebApp.Behaviors.OperationLoggingBehavior<TestQuery, Result<string>>>();
        var sut = new Dilcore.WebApp.Behaviors.OperationLoggingBehavior<TestQuery, Result<string>>(logger.Object);

        var result = await sut.Handle(new TestQuery(), _ => Task.FromResult(Result.Ok("ok")), CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        VerifyEventLogged(logger, eventId: 2100, level: LogLevel.Information, times: Times.Once());
        VerifyEventLogged(logger, eventId: 2101, level: LogLevel.Information, times: Times.Once());
    }

    [Test]
    public async Task Handle_CommandFailure_LogsStartAndFailure()
    {
        var logger = CreateLogger<Dilcore.WebApp.Behaviors.OperationLoggingBehavior<TestCommand, Result<string>>>();
        var sut = new Dilcore.WebApp.Behaviors.OperationLoggingBehavior<TestCommand, Result<string>>(logger.Object);

        var result = await sut.Handle(new TestCommand(), _ => Task.FromResult(Result.Fail<string>("nope")), CancellationToken.None);

        result.IsFailed.ShouldBeTrue();
        VerifyEventLogged(logger, eventId: 2110, level: LogLevel.Information, times: Times.Once());
        VerifyEventLogged(logger, eventId: 2112, level: LogLevel.Warning, times: Times.Once());
    }

    [Test]
    public async Task Handle_QueryException_LogsExceptionAndRethrows()
    {
        var logger = CreateLogger<Dilcore.WebApp.Behaviors.OperationLoggingBehavior<TestQuery, Result<string>>>();
        var sut = new Dilcore.WebApp.Behaviors.OperationLoggingBehavior<TestQuery, Result<string>>(logger.Object);

        var act = async () => await sut.Handle(
            new TestQuery(),
            _ => Task.FromException<Result<string>>(new InvalidOperationException("boom")),
            CancellationToken.None);

        await Should.ThrowAsync<InvalidOperationException>(act);
        VerifyEventLogged(logger, eventId: 2100, level: LogLevel.Information, times: Times.Once());
        VerifyEventLogged(logger, eventId: 2103, level: LogLevel.Error, times: Times.Once());
    }
}

