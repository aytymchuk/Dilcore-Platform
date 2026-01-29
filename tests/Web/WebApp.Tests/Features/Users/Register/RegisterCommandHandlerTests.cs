using Dilcore.Identity.Contracts.Profile;
using Dilcore.Identity.Contracts.Register;
using Dilcore.WebApi.Client.Clients;
using Dilcore.WebApp.Features.Users.Register;
using Dilcore.WebApp.Models.Users;
using Moq;
using Shouldly;

namespace Dilcore.WebApp.Tests.Features.Users.Register;

[TestFixture]
public class RegisterCommandHandlerTests
{
    private Mock<IIdentityClient> _identityClientMock = null!;
    private RegisterCommandHandler _handler = null!;

    [SetUp]
    public void Setup()
    {
        _identityClientMock = new Mock<IIdentityClient>();
        _handler = new RegisterCommandHandler(_identityClientMock.Object);
    }

    [Test]
    public async Task Handle_Should_Call_SafeRegisterUserAsync_With_Correct_UserDto()
    {
        // Arrange
        var parameters = new RegisterUserParameters
        {
            Email = "test@example.com",
            FirstName = "John",
            LastName = "Doe"
        };
        var command = new RegisterCommand(parameters);

        var expectedUserDto = new UserDto(
            Guid.NewGuid(),
            parameters.Email,
            parameters.FirstName,
            parameters.LastName,
            DateTime.UtcNow);

        _identityClientMock.Setup(x => x.RegisterUserAsync(It.IsAny<RegisterUserDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedUserDto);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.Id.ShouldBe(expectedUserDto.Id);
        result.Value.Email.ShouldBe(parameters.Email);
        result.Value.FirstName.ShouldBe(parameters.FirstName);
        result.Value.LastName.ShouldBe(parameters.LastName);

        _identityClientMock.Verify(x => x.RegisterUserAsync(
            It.Is<RegisterUserDto>(dto =>
                dto.Email == parameters.Email &&
                dto.FirstName == parameters.FirstName &&
                dto.LastName == parameters.LastName),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task Handle_Should_Return_Fail_When_Client_Fails()
    {
        // Arrange
        var parameters = new RegisterUserParameters
        {
            Email = "fail@example.com",
            FirstName = "Fail",
            LastName = "User"
        };
        var command = new RegisterCommand(parameters);
        var expectedError = "API Error";

        _identityClientMock.Setup(x => x.RegisterUserAsync(It.IsAny<RegisterUserDto>(), It.IsAny<CancellationToken>()))
             .ThrowsAsync(new Exception(expectedError));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailed.ShouldBeTrue();
        result.Errors.ShouldNotBeEmpty();
        result.Errors.ShouldContain(e => e.Message == expectedError);
    }
}
