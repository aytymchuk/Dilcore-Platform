using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Dilcore.Results.Abstractions;
using Dilcore.WebApi.IntegrationTests.Infrastructure;
using Shouldly;

namespace Dilcore.WebApi.IntegrationTests;

/// <summary>
/// Integration tests verifying Problem Details responses for exception handling.
/// </summary>
[TestFixture]
public class ProblemDetailsTests : BaseIntegrationTest
{
    private HttpClient _client = null!;

    [OneTimeSetUp]
    public void SetUpClient()
    {
        _client = Factory.CreateClient();
    }

    [OneTimeTearDown]
    public void TearDownClient()
    {
        _client.Dispose();
    }

    [Test]
    public async Task TestErrorEndpoint_NotFound_ReturnsProblemDetails()
    {
        // Act
        var response = await _client.GetAsync("/test/error/notfound");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        response.Content.Headers.ContentType?.MediaType.ShouldBe("application/problem+json");

        var problemDetails = await response.Content.ReadFromJsonAsync<JsonElement>();
        problemDetails.GetProperty("status").GetInt32().ShouldBe(404);
        problemDetails.GetProperty("title").GetString().ShouldBe("Resource Not Found");
        problemDetails.TryGetProperty("traceId", out _).ShouldBeTrue();
        problemDetails.TryGetProperty("errorCode", out var errorCode).ShouldBeTrue();
        errorCode.GetString().ShouldBe("NOT_FOUND");
    }

    [Test]
    public async Task TestErrorEndpoint_Validation_ReturnsBadRequest()
    {
        // Act
        var response = await _client.GetAsync("/test/error/validation");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        response.Content.Headers.ContentType?.MediaType.ShouldBe("application/problem+json");

        var problemDetails = await response.Content.ReadFromJsonAsync<JsonElement>();
        problemDetails.GetProperty("status").GetInt32().ShouldBe(400);
        problemDetails.TryGetProperty("errorCode", out var errorCode).ShouldBeTrue();
        errorCode.GetString().ShouldBe("VALIDATION_ERROR");
    }

    [Test]
    public async Task TestErrorEndpoint_Unauthorized_ReturnsUnauthorized()
    {
        // Act
        var response = await _client.GetAsync("/test/error/unauthorized");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        response.Content.Headers.ContentType?.MediaType.ShouldBe("application/problem+json");

        var problemDetails = await response.Content.ReadFromJsonAsync<JsonElement>();
        problemDetails.GetProperty("status").GetInt32().ShouldBe(401);
        problemDetails.TryGetProperty("errorCode", out var errorCode).ShouldBeTrue();
        errorCode.GetString().ShouldBe("UNAUTHORIZED");
    }

    /// <remarks>
    /// The "/test/error/conflict" endpoint throws InvalidOperationException, which maps to 500 UnexpectedError.
    /// This is intentional behavior as currently configured.
    /// </remarks>
    [Test]
    public async Task TestErrorEndpoint_Conflict_ReturnsInternalServerError()
    {
        // Act
        var response = await _client.GetAsync("/test/error/conflict");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.InternalServerError);
        response.Content.Headers.ContentType?.MediaType.ShouldBe("application/problem+json");

        var problemDetails = await response.Content.ReadFromJsonAsync<JsonElement>();
        problemDetails.GetProperty("status").GetInt32().ShouldBe(500);
        problemDetails.TryGetProperty("errorCode", out var errorCode).ShouldBeTrue();
        errorCode.GetString().ShouldBe("UNEXPECTED_ERROR");
    }

    [Test]
    public async Task TestErrorEndpoint_Timeout_ReturnsRequestTimeout()
    {
        // Act
        var response = await _client.GetAsync("/test/error/timeout");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.RequestTimeout);
        response.Content.Headers.ContentType?.MediaType.ShouldBe("application/problem+json");

        var problemDetails = await response.Content.ReadFromJsonAsync<JsonElement>();
        problemDetails.GetProperty("status").GetInt32().ShouldBe(408);
        problemDetails.TryGetProperty("errorCode", out var errorCode).ShouldBeTrue();
        errorCode.GetString().ShouldBe("TIMEOUT");
    }

    [Test]
    public async Task TestErrorEndpoint_UnknownType_ReturnsInternalServerError()
    {
        // Act
        var response = await _client.GetAsync("/test/error/unknown");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.InternalServerError);
        response.Content.Headers.ContentType?.MediaType.ShouldBe("application/problem+json");

        var problemDetails = await response.Content.ReadFromJsonAsync<JsonElement>();
        problemDetails.GetProperty("status").GetInt32().ShouldBe(500);
        problemDetails.TryGetProperty("errorCode", out var errorCode).ShouldBeTrue();
        errorCode.GetString().ShouldBe("UNEXPECTED_ERROR");
    }

    [Test]
    public async Task ProblemDetails_IncludesTimestamp()
    {
        // Act
        var response = await _client.GetAsync("/test/error/notfound");

        // Assert
        var problemDetails = await response.Content.ReadFromJsonAsync<JsonElement>();
        problemDetails.TryGetProperty("timestamp", out _).ShouldBeTrue();
    }

    [Test]
    public async Task ProblemDetails_IncludesTypeUri()
    {
        // Act
        var response = await _client.GetAsync("/test/error/notfound");

        // Assert
        var problemDetails = await response.Content.ReadFromJsonAsync<JsonElement>();
        problemDetails.TryGetProperty("type", out var typeUri).ShouldBeTrue();
        typeUri.GetString().ShouldBe($"{ProblemDetailsConstants.TypeBaseUri}/not-found");
    }

    [Test]
    public async Task NonExistentEndpoint_ReturnsNotFoundWithProblemDetails()
    {
        // Act
        var response = await _client.GetAsync("/nonexistent/endpoint");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        // Status code pages middleware should return Problem Details
        response.Content.Headers.ContentType?.MediaType.ShouldBe("application/problem+json");
    }
}
