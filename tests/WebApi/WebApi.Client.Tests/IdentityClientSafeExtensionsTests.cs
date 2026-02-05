using System.Net;
using System.Text.Json;
using Dilcore.Identity.Contracts.Profile;
using Dilcore.Identity.Contracts.Register;
using Dilcore.Results.Abstractions;
using Dilcore.WebApi.Client.Clients;
using Dilcore.WebApi.Client.Errors;
using Dilcore.WebApi.Client.Extensions;
using RichardSzalay.MockHttp;
using Shouldly;

namespace Dilcore.WebApi.Client.Tests;

[TestFixture]
public class IdentityClientSafeExtensionsTests
{
    private MockHttpMessageHandler _mockHttp = null!;
    private IIdentityClient _client = null!;

    [SetUp]
    public void Setup()
    {
        _mockHttp = new MockHttpMessageHandler();
        var httpClient = _mockHttp.ToHttpClient();
        httpClient.BaseAddress = new Uri("https://api.example.com");

        _client = Refit.RestService.For<IIdentityClient>(httpClient);
    }

    [TearDown]
    public void TearDown()
    {
        _mockHttp.Dispose();
    }

    #region SafeRegisterUserAsync Tests

    [Test]
    public async Task SafeRegisterUserAsync_ShouldReturnSuccess_WhenApiReturnsOk()
    {
        // Arrange
        var registerDto = new RegisterUserDto("test@example.com", "Test", "User");
        var expectedUser = new UserDto(
            Guid.CreateVersion7(),
            "test@example.com",
            "Test",
            "User",
            DateTime.UtcNow);

        _mockHttp.When(HttpMethod.Post, "https://api.example.com/users/register")
            .Respond("application/json", JsonSerializer.Serialize(expectedUser));

        // Act
        var result = await _client.SafeRegisterUserAsync(registerDto);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Email.ShouldBe("test@example.com");
        result.Value.FirstName.ShouldBe("Test");
        result.Value.LastName.ShouldBe("User");
    }

    [Test]
    public async Task SafeRegisterUserAsync_ShouldReturnFailure_WhenConflict()
    {
        // Arrange
        var registerDto = new RegisterUserDto("existing@example.com", "Existing", "User");
        var problemDetails = new
        {
            type = "https://api.dilcore.com/errors/conflict",
            title = "User Already Exists",
            status = 409,
            detail = "A user with email 'existing@example.com' already exists.",
            instance = "/users/register",
            traceId = "00-abc123-def456-01",
            errorCode = "USER_ALREADY_EXISTS",
            timestamp = DateTime.UtcNow
        };

        _mockHttp.When(HttpMethod.Post, "https://api.example.com/users/register")
            .Respond(HttpStatusCode.Conflict, "application/problem+json", JsonSerializer.Serialize(problemDetails));

        // Act
        var result = await _client.SafeRegisterUserAsync(registerDto);

        // Assert
        result.IsFailed.ShouldBeTrue();
        result.Errors.Count.ShouldBe(1);

        var error = result.Errors.First().ShouldBeOfType<ApiError>();
        error.StatusCode.ShouldBe(409);
        error.Code.ShouldBe("USER_ALREADY_EXISTS");
        error.Message.ShouldBe("A user with email 'existing@example.com' already exists.");
        error.Type.ShouldBe(ErrorType.Conflict);
        error.TraceId.ShouldBe("00-abc123-def456-01");
        error.Instance.ShouldBe("/users/register");
    }

    [Test]
    public async Task SafeRegisterUserAsync_ShouldReturnFailure_WhenValidationError()
    {
        // Arrange
        var registerDto = new RegisterUserDto("invalid-email", "Test", "User");
        var problemDetails = new
        {
            type = "https://api.dilcore.com/errors/validation",
            title = "Validation Failed",
            status = 422,
            detail = "One or more validation errors occurred.",
            instance = "/users/register",
            traceId = "00-validation-trace-01",
            errorCode = "VALIDATION_ERROR",
            timestamp = DateTime.UtcNow,
            errors = new Dictionary<string, string[]>
            {
                ["Email"] = new[] { "Email must be a valid email address." }
            }
        };

        _mockHttp.When(HttpMethod.Post, "https://api.example.com/users/register")
            .Respond(HttpStatusCode.UnprocessableEntity, "application/problem+json", JsonSerializer.Serialize(problemDetails));

        // Act
        var result = await _client.SafeRegisterUserAsync(registerDto);

        // Assert
        result.IsFailed.ShouldBeTrue();

        var error = result.Errors.First().ShouldBeOfType<ApiError>();
        error.StatusCode.ShouldBe(422);
        error.Code.ShouldBe("VALIDATION_ERROR");
        error.Message.ShouldBe("One or more validation errors occurred.");
        error.Type.ShouldBe(ErrorType.Validation);
        error.TraceId.ShouldBe("00-validation-trace-01");

        // Verify validation errors extraction
        error.Extensions.ShouldNotBeNull();
        error.Extensions.ContainsKey("errors").ShouldBeTrue();

        var errorsElement = (JsonElement)error.Extensions["errors"];
        var emailErrors = errorsElement.GetProperty("Email");
        emailErrors[0].GetString().ShouldBe("Email must be a valid email address.");
    }

    [Test]
    public async Task SafeRegisterUserAsync_ShouldReturnFailure_WhenUnauthorized()
    {
        // Arrange
        var registerDto = new RegisterUserDto("test@example.com", "Test", "User");
        var problemDetails = new
        {
            type = "https://api.dilcore.com/errors/unauthorized",
            title = "Unauthorized",
            status = 401,
            detail = "Authentication is required to access this resource.",
            instance = "/users/register",
            traceId = "00-auth-trace-01",
            errorCode = "UNAUTHORIZED"
        };

        _mockHttp.When(HttpMethod.Post, "https://api.example.com/users/register")
            .Respond(HttpStatusCode.Unauthorized, "application/problem+json", JsonSerializer.Serialize(problemDetails));

        // Act
        var result = await _client.SafeRegisterUserAsync(registerDto);

        // Assert
        result.IsFailed.ShouldBeTrue();

        var error = result.Errors.First().ShouldBeOfType<ApiError>();
        error.StatusCode.ShouldBe(401);
        error.Code.ShouldBe("UNAUTHORIZED");
        error.Type.ShouldBe(ErrorType.Unauthorized);
    }

    [Test]
    public async Task SafeRegisterUserAsync_ShouldReturnFailure_WhenInternalServerError()
    {
        // Arrange
        var registerDto = new RegisterUserDto("test@example.com", "Test", "User");
        var problemDetails = new
        {
            type = "https://api.dilcore.com/errors/unexpected",
            title = "Internal Server Error",
            status = 500,
            detail = "An unexpected error occurred while processing your request.",
            instance = "/users/register",
            traceId = "00-error-trace-01",
            errorCode = "UNEXPECTED_ERROR",
            timestamp = DateTime.UtcNow
        };

        _mockHttp.When(HttpMethod.Post, "https://api.example.com/users/register")
            .Respond(HttpStatusCode.InternalServerError, "application/problem+json", JsonSerializer.Serialize(problemDetails));

        // Act
        var result = await _client.SafeRegisterUserAsync(registerDto);

        // Assert
        result.IsFailed.ShouldBeTrue();

        var error = result.Errors.First().ShouldBeOfType<ApiError>();
        error.StatusCode.ShouldBe(500);
        error.Code.ShouldBe("UNEXPECTED_ERROR");
        error.Type.ShouldBe(ErrorType.Unexpected);
        error.TraceId.ShouldBe("00-error-trace-01");
    }

    [Test]
    public async Task SafeRegisterUserAsync_ShouldReturnFailure_WhenNoProblemDetails()
    {
        // Arrange
        var registerDto = new RegisterUserDto("test@example.com", "Test", "User");

        _mockHttp.When(HttpMethod.Post, "https://api.example.com/users/register")
            .Respond(HttpStatusCode.BadRequest, "text/plain", "Bad Request");

        // Act
        var result = await _client.SafeRegisterUserAsync(registerDto);

        // Assert
        result.IsFailed.ShouldBeTrue();

        var error = result.Errors.First().ShouldBeOfType<ApiError>();
        error.StatusCode.ShouldBe(400);
        error.Code.ShouldBe("INVALID_REQUEST"); // Default code for 400
        error.Type.ShouldBe(ErrorType.Validation);
    }

    #endregion

    #region SafeGetCurrentUserAsync Tests

    [Test]
    public async Task SafeGetCurrentUserAsync_ShouldReturnSuccess_WhenUserExists()
    {
        // Arrange
        var expectedUser = new UserDto(
            Guid.CreateVersion7(),
            "current@example.com",
            "Current",
            "User",
            DateTime.UtcNow);

        _mockHttp.When(HttpMethod.Get, "https://api.example.com/users/me")
            .Respond("application/json", JsonSerializer.Serialize(expectedUser));

        // Act
        var result = await _client.SafeGetCurrentUserAsync();

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Email.ShouldBe("current@example.com");
        result.Value.FirstName.ShouldBe("Current");
        result.Value.LastName.ShouldBe("User");
    }

    [Test]
    public async Task SafeGetCurrentUserAsync_ShouldReturnFailure_WhenNotFound()
    {
        // Arrange
        var problemDetails = new
        {
            type = "https://api.dilcore.com/errors/not-found",
            title = "User Not Found",
            status = 404,
            detail = "The current user could not be found.",
            instance = "/users/me",
            traceId = "00-notfound-trace-01",
            errorCode = "USER_NOT_FOUND",
            timestamp = DateTime.UtcNow
        };

        _mockHttp.When(HttpMethod.Get, "https://api.example.com/users/me")
            .Respond(HttpStatusCode.NotFound, "application/problem+json", JsonSerializer.Serialize(problemDetails));

        // Act
        var result = await _client.SafeGetCurrentUserAsync();

        // Assert
        result.IsFailed.ShouldBeTrue();

        var error = result.Errors.First().ShouldBeOfType<ApiError>();
        error.StatusCode.ShouldBe(404);
        error.Code.ShouldBe("USER_NOT_FOUND");
        error.Message.ShouldBe("The current user could not be found.");
        error.Type.ShouldBe(ErrorType.NotFound);
        error.TraceId.ShouldBe("00-notfound-trace-01");
    }

    [Test]
    public async Task SafeGetCurrentUserAsync_ShouldReturnFailure_WhenForbidden()
    {
        // Arrange
        var problemDetails = new
        {
            type = "https://api.dilcore.com/errors/forbidden",
            title = "Forbidden",
            status = 403,
            detail = "You do not have permission to access this resource.",
            instance = "/users/me",
            traceId = "00-forbidden-trace-01",
            errorCode = "FORBIDDEN"
        };

        _mockHttp.When(HttpMethod.Get, "https://api.example.com/users/me")
            .Respond(HttpStatusCode.Forbidden, "application/problem+json", JsonSerializer.Serialize(problemDetails));

        // Act
        var result = await _client.SafeGetCurrentUserAsync();

        // Assert
        result.IsFailed.ShouldBeTrue();

        var error = result.Errors.First().ShouldBeOfType<ApiError>();
        error.StatusCode.ShouldBe(403);
        error.Code.ShouldBe("FORBIDDEN");
        error.Type.ShouldBe(ErrorType.Forbidden);
    }

    #endregion

    #region Network Error Tests

    [Test]
    public async Task SafeRegisterUserAsync_ShouldReturnFailure_OnNetworkError()
    {
        // Arrange
        var registerDto = new RegisterUserDto("test@example.com", "Test", "User");

        _mockHttp.When(HttpMethod.Post, "https://api.example.com/users/register")
            .Throw(new HttpRequestException("Network error"));

        // Act
        var result = await _client.SafeRegisterUserAsync(registerDto);

        // Assert
        result.IsFailed.ShouldBeTrue();

        var error = result.Errors.First().ShouldBeOfType<ApiError>();
        error.StatusCode.ShouldBe(503);
        error.Code.ShouldBe("NETWORK_ERROR");
        error.Type.ShouldBe(ErrorType.Unexpected);
    }

    [Test]
    public async Task SafeGetCurrentUserAsync_ShouldReturnFailure_OnTimeout()
    {
        // Arrange
        _mockHttp.When(HttpMethod.Get, "https://api.example.com/users/me")
            .Throw(new TaskCanceledException("Request timeout"));

        // Act
        var result = await _client.SafeGetCurrentUserAsync();

        // Assert
        result.IsFailed.ShouldBeTrue();

        var error = result.Errors.First().ShouldBeOfType<ApiError>();
        error.StatusCode.ShouldBe(408);
        error.Code.ShouldBe("TIMEOUT");
        error.Type.ShouldBe(ErrorType.Unexpected);
    }

    #endregion
}
