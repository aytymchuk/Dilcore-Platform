using System.Net;
using Dilcore.Identity.Contracts.Profile;
using Dilcore.WebApi.Client.Clients;
using Dilcore.WebApp.Features.Users.CurrentUser;
using Moq;
using Refit;
using Shouldly;

namespace Dilcore.WebApp.Tests.Features.Users.CurrentUser;

[TestFixture]
public class GetCurrentUserQueryHandlerTests
{
    private Mock<IIdentityClient> _identityClientMock;
    private GetCurrentUserQueryHandler _handler;

    [SetUp]
    public void Setup()
    {
        _identityClientMock = new Mock<IIdentityClient>();
        _handler = new GetCurrentUserQueryHandler(_identityClientMock.Object);
    }

    [Test]
    public async Task Handle_Should_Return_User_When_Client_Returns_Success()
    {
        // Arrange
        var query = new GetCurrentUserQuery();
        var expectedUserDto = new UserDto(
            Guid.NewGuid(),
            "test@example.com",
            "John",
            "Doe",
            DateTime.UtcNow);

        _identityClientMock.Setup(x => x.GetCurrentUserAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedUserDto);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(expectedUserDto);
    }

    [Test]
    public async Task Handle_Should_Return_UserNotFoundError_When_Client_Returns_404()
    {
        // Arrange
        var query = new GetCurrentUserQuery();
        var apiException = await ApiException.Create(
            new HttpRequestMessage(),
            HttpMethod.Get,
            new HttpResponseMessage(HttpStatusCode.NotFound) { Content = new StringContent("") },
            new RefitSettings());

        _identityClientMock.Setup(x => x.GetCurrentUserAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(apiException);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsFailed.ShouldBeTrue();
        result.HasError<UserNotFoundError>().ShouldBeTrue();
    }

    [Test]
    public async Task Handle_Should_Return_Original_Error_When_Client_Returns_Other_Error()
    {
        // Arrange
        var query = new GetCurrentUserQuery();
        
        _identityClientMock.Setup(x => x.GetCurrentUserAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Fail"));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsFailed.ShouldBeTrue();
        result.HasError<UserNotFoundError>().ShouldBeFalse();
        // Since we throw generic Exception, usage of ApiErrorHelper.CreateUnexpectedError will set message to exception message
        result.Errors.ShouldContain(e => e.Message == "Fail");
    }
}
