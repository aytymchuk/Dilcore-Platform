using Dilcore.Authentication.Abstractions;
using Microsoft.Extensions.Logging;
using Moq;

namespace Dilcore.Authentication.Orleans.Extensions.Tests;

[TestFixture]
public class UserIncomingGrainCallFilterTests
{
    private Mock<IIncomingGrainCallContext> _contextMock;
    private Mock<ILogger<UserIncomingGrainCallFilter>> _loggerMock;
    private UserIncomingGrainCallFilter _filter;

    [SetUp]
    public void SetUp()
    {
        _contextMock = new Mock<IIncomingGrainCallContext>();
        _loggerMock = new Mock<ILogger<UserIncomingGrainCallFilter>>();
        _filter = new UserIncomingGrainCallFilter(_loggerMock.Object);
        
        // Ensure context mocks return some values
        _contextMock.Setup(c => c.Grain).Returns(new Mock<IGrain>().Object);
        _contextMock.Setup(c => c.InterfaceMethod).Returns(typeof(IGrain).GetMethods().FirstOrDefault());

        // Allow logging
        _loggerMock.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
    }

    [TearDown]
    public void TearDown()
    {
        OrleansUserContextAccessor.SetUserContext(null);
    }

    [Test]
    public async Task Invoke_Should_Extract_UserContext_And_Log_When_Present()
    {
        // Arrange
        var userId = "user-123";
        var email = "test@example.com";
        var userContext = new UserContext(userId, email, "Test User", [], []);
        OrleansUserContextAccessor.SetUserContext(userContext);

        // Act
        await _filter.Invoke(_contextMock.Object);

        // Assert
        _contextMock.Verify(c => c.Invoke(), Times.Once);
        
        // Verify logger was called with extracted context info
        // Since LoggerMessage source generator is used, we verify generic log call
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains(userId) && v.ToString().Contains(email)),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Test]
    public async Task Invoke_Should_Log_NotFound_When_UserContext_Is_Null()
    {
        // Arrange
        OrleansUserContextAccessor.SetUserContext(null);

        // Act
        await _filter.Invoke(_contextMock.Object);

        // Assert
        _contextMock.Verify(c => c.Invoke(), Times.Once);

        // Verify logger was called with "not found" message
         _loggerMock.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("No user context found")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
