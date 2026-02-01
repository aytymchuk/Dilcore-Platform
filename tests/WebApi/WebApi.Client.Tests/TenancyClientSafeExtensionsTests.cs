using System.Net;
using System.Text.Json;
using Dilcore.Results.Abstractions;
using Dilcore.Tenancy.Contracts.Tenants;
using Dilcore.Tenancy.Contracts.Tenants.Create;
using Dilcore.WebApi.Client.Clients;
using Dilcore.WebApi.Client.Errors;
using Dilcore.WebApi.Client.Extensions;
using RichardSzalay.MockHttp;
using Shouldly;

namespace Dilcore.WebApi.Client.Tests;

[TestFixture]
public class TenancyClientSafeExtensionsTests
{
    private MockHttpMessageHandler _mockHttp = null!;
    private HttpClient _httpClient = null!;
    private ITenancyClient _client = null!;

    [SetUp]
    public void Setup()
    {
        _mockHttp = new MockHttpMessageHandler();
        _httpClient = _mockHttp.ToHttpClient();
        _httpClient.BaseAddress = new Uri("https://api.example.com");

        _client = Refit.RestService.For<ITenancyClient>(_httpClient);
    }

    [TearDown]
    public void TearDown()
    {
        _httpClient.Dispose();
        _mockHttp.Dispose();
    }

    #region SafeCreateTenantAsync Tests

    [Test]
    public async Task SafeCreateTenantAsync_ShouldReturnSuccess_WhenApiReturnsOk()
    {
        // Arrange
        var request = new CreateTenantDto
        {
            Name = "Test Tenant",
            Description = "Test Tenant Description",
        };
        var expectedTenant = new TenantDto
        {
            Name = "Test Tenant",
            SystemName = "test-tenant",
            Description = "Test Tenant Description",
            CreatedAt = DateTime.UtcNow
        };

        _mockHttp.When(HttpMethod.Post, "https://api.example.com/tenants")
            .Respond("application/json", JsonSerializer.Serialize(expectedTenant));

        // Act
        var result = await _client.SafeCreateTenantAsync(request);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.SystemName.ShouldBe("test-tenant");
        result.Value.Name.ShouldBe("Test Tenant");
    }

    [Test]
    public async Task SafeCreateTenantAsync_ShouldReturnFailure_WhenConflict()
    {
        // Arrange
        var request = new CreateTenantDto
        {
            Name = "Test Tenant",
            Description = "Test Tenant Description",
        };
        
        var problemDetails = new
        {
            type = "https://api.dilcore.com/errors/conflict",
            title = "Tenant Already Exists",
            status = 409,
            detail = "A tenant with slug 'existing-tenant' already exists.",
            instance = "/tenants",
            traceId = "00-tenant-conflict-01",
            errorCode = "TENANT_ALREADY_EXISTS",
            timestamp = DateTime.UtcNow
        };

        _mockHttp.When(HttpMethod.Post, "https://api.example.com/tenants")
            .Respond(HttpStatusCode.Conflict, "application/problem+json", JsonSerializer.Serialize(problemDetails));

        // Act
        var result = await _client.SafeCreateTenantAsync(request);

        // Assert
        result.IsFailed.ShouldBeTrue();
        result.Errors.Count.ShouldBe(1);

        var error = result.Errors.First().ShouldBeOfType<ApiError>();
        error.StatusCode.ShouldBe(409);
        error.Code.ShouldBe("TENANT_ALREADY_EXISTS");
        error.Message.ShouldBe("A tenant with slug 'existing-tenant' already exists.");
        error.Type.ShouldBe(ErrorType.Conflict);
        error.TraceId.ShouldBe("00-tenant-conflict-01");
    }

    [Test]
    public async Task SafeCreateTenantAsync_ShouldReturnFailure_WhenValidationError()
    {
        // Arrange
        var request = new CreateTenantDto
        {
            Name = "",
            Description = ""
        };
        var problemDetails = new
        {
            type = "https://api.dilcore.com/errors/validation",
            title = "Validation Failed",
            status = 422,
            detail = "One or more validation errors occurred.",
            instance = "/tenants",
            traceId = "00-tenant-validation-01",
            errorCode = "VALIDATION_ERROR",
            timestamp = DateTime.UtcNow,
            errors = new Dictionary<string, string[]>
            {
                ["Name"] = new[] { "Name is required." },
                ["Description"] = new[] { "Description is required." }
            }
        };

        _mockHttp.When(HttpMethod.Post, "https://api.example.com/tenants")
            .Respond(HttpStatusCode.UnprocessableEntity, "application/problem+json", JsonSerializer.Serialize(problemDetails));

        // Act
        var result = await _client.SafeCreateTenantAsync(request);

        // Assert
        result.IsFailed.ShouldBeTrue();
        
        var error = result.Errors.First().ShouldBeOfType<ApiError>();
        error.StatusCode.ShouldBe(422);
        error.Code.ShouldBe("VALIDATION_ERROR");
        error.Type.ShouldBe(ErrorType.Validation);
    }

    #endregion

    #region SafeGetTenantAsync Tests

    [Test]
    public async Task SafeGetTenantAsync_ShouldReturnSuccess_WhenTenantExists()
    {
        // Arrange
        var expectedTenant = new TenantDto
        {
            Name = "Current Tenant Display",
            Description = "Current tenant description",
            SystemName = "current-tenant",
            CreatedAt = DateTime.UtcNow
        };

        _mockHttp.When(HttpMethod.Get, "https://api.example.com/tenants")
            .Respond("application/json", JsonSerializer.Serialize(expectedTenant));

        // Act
        var result = await _client.SafeGetTenantAsync();

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.SystemName.ShouldBe("current-tenant");
        result.Value.Name.ShouldBe("Current Tenant Display");
    }

    [Test]
    public async Task SafeGetTenantAsync_ShouldReturnFailure_WhenNotFound()
    {
        // Arrange
        var problemDetails = new
        {
            type = "https://api.dilcore.com/errors/not-found",
            title = "Tenant Not Found",
            status = 404,
            detail = "The requested tenant could not be found.",
            instance = "/tenants",
            traceId = "00-tenant-notfound-01",
            errorCode = "TENANT_NOT_FOUND",
            timestamp = DateTime.UtcNow
        };

        _mockHttp.When(HttpMethod.Get, "https://api.example.com/tenants")
            .Respond(HttpStatusCode.NotFound, "application/problem+json", JsonSerializer.Serialize(problemDetails));

        // Act
        var result = await _client.SafeGetTenantAsync();

        // Assert
        result.IsFailed.ShouldBeTrue();
        
        var error = result.Errors.First().ShouldBeOfType<ApiError>();
        error.StatusCode.ShouldBe(404);
        error.Code.ShouldBe("TENANT_NOT_FOUND");
        error.Message.ShouldBe("The requested tenant could not be found.");
        error.Type.ShouldBe(ErrorType.NotFound);
        error.TraceId.ShouldBe("00-tenant-notfound-01");
    }

    [Test]
    public async Task SafeGetTenantAsync_ShouldReturnFailure_WhenUnauthorized()
    {
        // Arrange
        var problemDetails = new
        {
            type = "https://api.dilcore.com/errors/unauthorized",
            title = "Unauthorized",
            status = 401,
            detail = "You must be authenticated to access tenant information.",
            instance = "/tenants",
            traceId = "00-tenant-auth-01",
            errorCode = "UNAUTHORIZED"
        };

        _mockHttp.When(HttpMethod.Get, "https://api.example.com/tenants")
            .Respond(HttpStatusCode.Unauthorized, "application/problem+json", JsonSerializer.Serialize(problemDetails));

        // Act
        var result = await _client.SafeGetTenantAsync();

        // Assert
        result.IsFailed.ShouldBeTrue();
        
        var error = result.Errors.First().ShouldBeOfType<ApiError>();
        error.StatusCode.ShouldBe(401);
        error.Code.ShouldBe("UNAUTHORIZED");
        error.Type.ShouldBe(ErrorType.Unauthorized);
    }

    [Test]
    public async Task SafeGetTenantAsync_ShouldReturnFailure_WhenServiceUnavailable()
    {
        // Arrange
        var problemDetails = new
        {
            type = "https://api.dilcore.com/errors/service-unavailable",
            title = "Service Unavailable",
            status = 503,
            detail = "The tenant service is temporarily unavailable.",
            instance = "/tenants",
            traceId = "00-service-unavailable-01",
            errorCode = "TIMEOUT"
        };

        _mockHttp.When(HttpMethod.Get, "https://api.example.com/tenants")
            .Respond(HttpStatusCode.ServiceUnavailable, "application/problem+json", JsonSerializer.Serialize(problemDetails));

        // Act
        var result = await _client.SafeGetTenantAsync();

        // Assert
        result.IsFailed.ShouldBeTrue();
        
        var error = result.Errors.First().ShouldBeOfType<ApiError>();
        error.StatusCode.ShouldBe(503);
        error.Code.ShouldBe("TIMEOUT");
        error.Type.ShouldBe(ErrorType.Unexpected);
    }

    #endregion

    #region Network Error Tests

    [Test]
    public async Task SafeCreateTenantAsync_ShouldReturnFailure_OnNetworkError()
    {
        // Arrange
        var request = new CreateTenantDto
        {
            Name = "Test Tenant",
            Description = "test-tenant"
        };

        _mockHttp.When(HttpMethod.Post, "https://api.example.com/tenants")
            .Throw(new HttpRequestException("Connection refused"));

        // Act
        var result = await _client.SafeCreateTenantAsync(request);

        // Assert
        result.IsFailed.ShouldBeTrue();
        
        var error = result.Errors.First().ShouldBeOfType<ApiError>();
        error.StatusCode.ShouldBe(503);
        error.Code.ShouldBe("NETWORK_ERROR");
        error.Type.ShouldBe(ErrorType.Unexpected);
    }

    [Test]
    public async Task SafeGetTenantAsync_ShouldReturnFailure_OnTimeout()
    {
        // Arrange
        _mockHttp.When(HttpMethod.Get, "https://api.example.com/tenants")
            .Throw(new TaskCanceledException("Operation timed out"));

        // Act
        var result = await _client.SafeGetTenantAsync();

        // Assert
        result.IsFailed.ShouldBeTrue();
        
        var error = result.Errors.First().ShouldBeOfType<ApiError>();
        error.StatusCode.ShouldBe(408);
        error.Code.ShouldBe("TIMEOUT");
        error.Type.ShouldBe(ErrorType.Unexpected);
    }

    [Test]
    public async Task SafeGetTenantAsync_ShouldReturnFailure_OnCancellation()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        cts.Cancel();
        var token = cts.Token;

        _mockHttp.When(HttpMethod.Get, "https://api.example.com/tenants")
            .Throw(new TaskCanceledException("The API request was cancelled.", null, token));

        // Act
        var result = await _client.SafeGetTenantAsync();

        // Assert
        result.IsFailed.ShouldBeTrue();

        var error = result.Errors.First().ShouldBeOfType<ApiError>();
        error.StatusCode.ShouldBe(400); // Bad Request (cancellation)
        error.Code.ShouldBe("CANCELLED");
        error.Type.ShouldBe(ErrorType.Unexpected);
    }

    #endregion
}
