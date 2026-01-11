using Dilcore.MediatR.Extensions;
using FluentResults;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

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

    public class TestRequest : IRequest<Result>
    {
    }
}