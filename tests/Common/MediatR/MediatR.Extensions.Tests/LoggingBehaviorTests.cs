using Dilcore.MediatR.Extensions;
using FluentResults;
using Microsoft.Extensions.Logging;
using Moq;

using Shouldly;

namespace MediatR.Extensions.Tests;

[TestFixture]
public class LoggingBehaviorTests
{
    private Mock<ILogger<LoggingBehavior<TestRequest, Result>>> _loggerMock;
    private LoggingBehavior<TestRequest, Result> _behavior;

    [SetUp]
    public void SetUp()
    {
        _loggerMock = new Mock<ILogger<LoggingBehavior<TestRequest, Result>>>();
        _loggerMock.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
        _behavior = new LoggingBehavior<TestRequest, Result>(_loggerMock.Object);
    }

    [Test]
    public async Task Handle_ShouldLogException_WhenResultHasExceptionalError()
    {
        // Arrange
        var request = new TestRequest();
        var exception = new Exception("Test fatal error");
        var result = Result.Fail(new ExceptionalError("Operation failed", exception));

        // Act
        await _behavior.Handle(request, (ct) => Task.FromResult(result), CancellationToken.None);

        // Assert
        // Verify LogError was called with the exception
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        // Verify LogWarning is still called for the summary
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Test]
    public async Task Handle_ShouldLogException_WhenResultHasNestedExceptionalError()
    {
        // Arrange
        var request = new TestRequest();
        var innerException = new Exception("Inner fatal error");
        var error = new Error("Top level error")
            .CausedBy(new ExceptionalError("Inner operation failed", innerException));
        var result = Result.Fail(error);

        // Act
        await _behavior.Handle(request, (ct) => Task.FromResult(result), CancellationToken.None);

        // Assert
        // Verify LogError was called with the inner exception
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                innerException,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        // Verify LogWarning contains both messages
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Top level error") && v.ToString()!.Contains("Inner operation failed")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Test]
    public async Task Handle_ShouldLogInformation_WhenResultIsOk()
    {
        // Arrange
        var request = new TestRequest();
        var result = Result.Ok();

        // Act
        await _behavior.Handle(request, (ct) => Task.FromResult(result), CancellationToken.None);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Exactly(2));
    }

    [Test]
    public async Task Handle_ShouldLogWarning_WhenResultIsFail_AndNotExceptional()
    {
        // Arrange
        var request = new TestRequest();
        var result = Result.Fail("Some validation error");

        // Act
        await _behavior.Handle(request, (ct) => Task.FromResult(result), CancellationToken.None);

        // Assert
        // Verify LogWarning is called with the error message
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Some validation error")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        // Verify LogError is NOT called
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never);
    }

    [Test]
    public async Task Handle_ShouldLogInformation_WhenResponseIsNotResult()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<LoggingBehavior<TestRequestWithResponse, TestResponse>>>();
        loggerMock.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
        var behavior = new LoggingBehavior<TestRequestWithResponse, TestResponse>(loggerMock.Object);

        var request = new TestRequestWithResponse();
        var response = new TestResponse();

        // Act
        await behavior.Handle(request, (ct) => Task.FromResult(response), CancellationToken.None);

        // Assert
        loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Exactly(2));

        // Verify no result-specific logging (Warning/Error) occurred
        loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never);

        loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never);
    }

    [Test]
    public async Task Handle_ShouldReThrowAndLogException_WhenHandlerThrows()
    {
        // Arrange
        var request = new TestRequest();
        var exception = new InvalidOperationException("Handler exploded");
        RequestHandlerDelegate<Result> next = (ct) => throw exception;

        // Act & Assert
        await Should.ThrowAsync<InvalidOperationException>(async () =>
            await _behavior.Handle(request, next, CancellationToken.None));

        // Verify LogError was called with the exception
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    public class TestRequest : IRequest<Result>
    {
    }

    public class TestRequestWithResponse : IRequest<TestResponse>
    {
    }

    public class TestResponse
    {
    }
}