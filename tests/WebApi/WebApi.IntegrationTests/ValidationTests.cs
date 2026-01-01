using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Shouldly;

namespace Dilcore.WebApi.IntegrationTests;

/// <summary>
/// Integration tests verifying FluentValidation endpoint filter behavior.
/// </summary>
[TestFixture]
public class ValidationTests
{
    private CustomWebApplicationFactory _factory = null!;
    private HttpClient _client = null!;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _factory = new CustomWebApplicationFactory();
        _client = _factory.CreateClient();
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        _client.Dispose();
        _factory.Dispose();
    }

    [Test]
    public async Task ValidRequest_PassesValidation_ReturnsOk()
    {
        // Arrange
        var validDto = new
        {
            name = "John Doe",
            email = "john@example.com",
            age = 30,
            startDate = "2026-01-01"
        };

        var content = new StringContent(JsonSerializer.Serialize(validDto), Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/test/validation", content);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<JsonElement>();
        result.GetProperty("message").GetString().ShouldBe("Validation passed successfully!");
    }

    [Test]
    public async Task InvalidRequest_MissingRequiredFields_ReturnsBadRequest()
    {
        // Arrange - missing all required fields
        var invalidDto = new { };

        var content = new StringContent(JsonSerializer.Serialize(invalidDto), Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/test/validation", content);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        response.Content.Headers.ContentType?.MediaType.ShouldBe("application/problem+json");
    }

    [Test]
    public async Task InvalidRequest_ValidationErrors_ContainsCorrectErrorCode()
    {
        // Arrange - missing required fields
        var invalidDto = new
        {
            name = "", // empty name
            email = "invalid-email", // invalid email format
            age = 200, // out of range
            startDate = "2026-01-01"
        };

        var content = new StringContent(JsonSerializer.Serialize(invalidDto), Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/test/validation", content);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        var problemDetails = await response.Content.ReadFromJsonAsync<JsonElement>();
        problemDetails.GetProperty("status").GetInt32().ShouldBe(400);
        problemDetails.GetProperty("title").GetString().ShouldBe("Validation Failed");
        problemDetails.TryGetProperty("errorCode", out var errorCode).ShouldBeTrue();
        errorCode.GetString().ShouldBe("DATA_VALIDATION_FAILED");
    }

    [Test]
    public async Task InvalidRequest_ValidationErrors_ContainsErrorsProperty()
    {
        // Arrange
        var invalidDto = new
        {
            name = "", // empty name
            email = "invalid-email",
            age = 30,
            startDate = "2026-01-01"
        };

        var content = new StringContent(JsonSerializer.Serialize(invalidDto), Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/test/validation", content);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        var problemDetails = await response.Content.ReadFromJsonAsync<JsonElement>();
        problemDetails.TryGetProperty("errors", out var errors).ShouldBeTrue();
        errors.ValueKind.ShouldBe(JsonValueKind.Object);
    }

    [Test]
    public async Task InvalidRequest_NameTooShort_ReturnsSpecificError()
    {
        // Arrange
        var invalidDto = new
        {
            name = "A", // too short, min 2 chars
            email = "valid@example.com",
            age = 30,
            startDate = "2026-01-01"
        };

        var content = new StringContent(JsonSerializer.Serialize(invalidDto), Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/test/validation", content);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        var problemDetails = await response.Content.ReadFromJsonAsync<JsonElement>();
        var errors = problemDetails.GetProperty("errors");
        errors.TryGetProperty("name", out var nameErrors).ShouldBeTrue();
        nameErrors.GetArrayLength().ShouldBeGreaterThan(0);
        nameErrors[0].GetString().ShouldContain("2 characters");
    }

    [Test]
    public async Task InvalidRequest_AgeOutOfRange_ReturnsSpecificError()
    {
        // Arrange
        var invalidDto = new
        {
            name = "John Doe",
            email = "valid@example.com",
            age = 200, // out of range (0-150)
            startDate = "2026-01-01"
        };

        var content = new StringContent(JsonSerializer.Serialize(invalidDto), Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/test/validation", content);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        var problemDetails = await response.Content.ReadFromJsonAsync<JsonElement>();
        var errors = problemDetails.GetProperty("errors");
        errors.TryGetProperty("age", out var ageErrors).ShouldBeTrue();
        ageErrors.GetArrayLength().ShouldBeGreaterThan(0);
    }

    [Test]
    public async Task InvalidRequest_InvalidEmail_ReturnsSpecificError()
    {
        // Arrange
        var invalidDto = new
        {
            name = "John Doe",
            email = "not-an-email",
            age = 30,
            startDate = "2026-01-01"
        };

        var content = new StringContent(JsonSerializer.Serialize(invalidDto), Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/test/validation", content);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        var problemDetails = await response.Content.ReadFromJsonAsync<JsonElement>();
        var errors = problemDetails.GetProperty("errors");
        errors.TryGetProperty("email", out var emailErrors).ShouldBeTrue();
        emailErrors.GetArrayLength().ShouldBeGreaterThan(0);
    }

    [Test]
    public async Task InvalidRequest_MultipleErrors_GroupedByField()
    {
        // Arrange - multiple validation errors
        var invalidDto = new
        {
            name = "", // empty (NotEmpty + MinLength errors)
            email = "x", // invalid email (EmailAddress error)
            age = -5, // out of range (InclusiveBetween error)
            startDate = "2026-01-01",
            endDate = "2025-01-01" // before start date
        };

        var content = new StringContent(JsonSerializer.Serialize(invalidDto), Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/test/validation", content);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        var problemDetails = await response.Content.ReadFromJsonAsync<JsonElement>();
        var errors = problemDetails.GetProperty("errors");

        // Should have multiple fields with errors
        errors.TryGetProperty("name", out _).ShouldBeTrue();
        errors.TryGetProperty("email", out _).ShouldBeTrue();
        errors.TryGetProperty("age", out _).ShouldBeTrue();
        errors.TryGetProperty("endDate", out _).ShouldBeTrue();
    }

    [Test]
    public async Task InvalidRequest_InvalidPhoneFormat_ReturnsError()
    {
        // Arrange
        var invalidDto = new
        {
            name = "John Doe",
            email = "valid@example.com",
            age = 30,
            startDate = "2026-01-01",
            phoneNumber = "invalid-phone" // should be E.164 format
        };

        var content = new StringContent(JsonSerializer.Serialize(invalidDto), Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/test/validation", content);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        var problemDetails = await response.Content.ReadFromJsonAsync<JsonElement>();
        var errors = problemDetails.GetProperty("errors");
        errors.TryGetProperty("phoneNumber", out _).ShouldBeTrue();
    }

    [Test]
    public async Task ValidRequest_OptionalFields_PassesValidation()
    {
        // Arrange - valid with optional fields
        var validDto = new
        {
            name = "John Doe",
            email = "john@example.com",
            age = 30,
            startDate = "2026-01-01",
            endDate = "2026-12-31",
            phoneNumber = "+12345678901",
            website = "https://example.com",
            tags = new[] { "tag1", "tag2" }
        };

        var content = new StringContent(JsonSerializer.Serialize(validDto), Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/test/validation", content);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Test]
    public async Task InvalidRequest_TooManyTags_ReturnsError()
    {
        // Arrange
        var invalidDto = new
        {
            name = "John Doe",
            email = "john@example.com",
            age = 30,
            startDate = "2026-01-01",
            tags = new[] { "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11" } // max 10
        };

        var content = new StringContent(JsonSerializer.Serialize(invalidDto), Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/test/validation", content);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        var problemDetails = await response.Content.ReadFromJsonAsync<JsonElement>();
        var errors = problemDetails.GetProperty("errors");
        errors.TryGetProperty("tags", out _).ShouldBeTrue();
    }

    [Test]
    public async Task ProblemDetails_HasCorrectType()
    {
        // Arrange
        var invalidDto = new
        {
            name = "",
            email = "invalid",
            age = 30,
            startDate = "2026-01-01"
        };

        var content = new StringContent(JsonSerializer.Serialize(invalidDto), Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/test/validation", content);

        // Assert
        var problemDetails = await response.Content.ReadFromJsonAsync<JsonElement>();
        problemDetails.TryGetProperty("type", out var type).ShouldBeTrue();
        type.GetString().ShouldBe($"{Constants.ProblemDetails.TypeBaseUri}/data-validation-failed");
    }

    [Test]
    public async Task ProblemDetails_IncludesTraceId()
    {
        // Arrange
        var invalidDto = new
        {
            name = "",
            email = "invalid",
            age = 30,
            startDate = "2026-01-01"
        };

        var content = new StringContent(JsonSerializer.Serialize(invalidDto), Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/test/validation", content);

        // Assert
        var problemDetails = await response.Content.ReadFromJsonAsync<JsonElement>();
        problemDetails.TryGetProperty("traceId", out _).ShouldBeTrue();
    }
}
