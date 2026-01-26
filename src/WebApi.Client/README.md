# Dilcore WebAPI Client

A type-safe HTTP client library for the Dilcore Platform API built with [Refit](https://github.com/reactiveui/refit).

## Features

- ✨ **Type-safe REST clients** - Refit-generated clients from interface definitions
- 🔄 **Automatic retry policies** - Polly-based exponential backoff for transient failures
- 🔌 **Extensible** - Support for custom delegating handlers (auth, correlation IDs, etc.)
- 🎯 **Strongly typed** - Full IntelliSense support with compile-time safety
- 📦 **Modular** - Separate clients for Identity and Tenancy modules
- ✅ **Result-based error handling** - Optional `Safe*` methods with FluentResults and ProblemDetails support

## Installation

Add the package from GitHub Packages:

```bash
dotnet add package Dilcore.WebApi.Client
```

## Quick Start

### Basic Usage

```csharp
using Dilcore.WebApi.Client;
using Dilcore.WebApi.Client.Clients;
using Microsoft.Extensions.DependencyInjection;

// Register the clients
services.AddPlatformApiClients(options =>
{
    options.BaseAddress = new Uri("https://api.dilcore.com");
    options.Timeout = TimeSpan.FromSeconds(30);
    options.RetryCount = 3;
    options.RetryDelaySeconds = 2.0;
});

// Inject and use individual clients
public class UserService
{
    private readonly IIdentityClient _identityClient;

    public UserService(IIdentityClient identityClient)
    {
        _identityClient = identityClient;
    }

    public async Task RegisterUser()
    {
        var dto = new RegisterUserDto("user@example.com", "John", "Doe");
        var user = await _identityClient.RegisterUserAsync(dto);
        Console.WriteLine($"Registered user: {user.Email}");
    }
}

public class TenantService
{
    private readonly ITenancyClient _tenancyClient;

    public TenantService(ITenancyClient tenancyClient)
    {
        _tenancyClient = tenancyClient;
    }

    public async Task CreateTenant()
    {
        var command = new CreateTenantCommand("My Tenant", "my-tenant");
        var tenant = await _tenancyClient.CreateTenantAsync(command);
        Console.WriteLine($"Created tenant: {tenant.Name}");
    }
}
```

### Adding Custom Delegating Handlers

You can add custom handlers (authentication, logging, etc.) by extending the registration:

```csharp
// Custom handler for adding authentication headers
public class AuthHeaderHandler : DelegatingHandler
{
    private readonly IAuthTokenStore _tokenStore;

    public AuthHeaderHandler(IAuthTokenStore tokenStore)
    {
        _tokenStore = tokenStore;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var token = await _tokenStore.GetTokenAsync();
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return await base.SendAsync(request, cancellationToken);
    }
}

// Register with custom handlers
services.AddTransient<AuthHeaderHandler>();

services.AddPlatformApiClients(
    options =>
    {
        options.BaseAddress = new Uri("https://api.dilcore.com");
    },
    configureClient: builder => builder.AddHttpMessageHandler<AuthHeaderHandler>()
);
```

## Available Clients

Each client can be injected independently:

### Identity Client

```csharp
public class UserService
{
    private readonly IIdentityClient _client;

    public UserService(IIdentityClient client)
    {
        _client = client;
    }

    public async Task Example()
    {
        // Register a new user
        var registerDto = new RegisterUserDto("user@example.com", "John", "Doe");
        var user = await _client.RegisterUserAsync(registerDto);

        // Get current user profile
        var currentUser = await _client.GetCurrentUserAsync();
    }
}
```

### Tenancy Client

```csharp
public class TenantService
{
    private readonly ITenancyClient _client;

    public TenantService(ITenancyClient client)
    {
        _client = client;
    }

    public async Task Example()
    {
        // Create a new tenant
        var command = new CreateTenantCommand("My Tenant", "my-tenant");
        var tenant = await _client.CreateTenantAsync(command);

        // Get current tenant (based on x-tenant header)
        var currentTenant = await _client.GetTenantAsync();
    }
}
```

## Configuration Options

| Option | Type | Default | Description |
|--------|------|---------|-------------|
| `BaseAddress` | `Uri` | Required | Base URL of the Platform API |
| `Timeout` | `TimeSpan` | 30 seconds | HTTP request timeout |
| `RetryCount` | `int` | 3 | Number of retry attempts for transient failures |
| `RetryDelaySeconds` | `double` | 2.0 | Base delay between retries (exponential backoff) |

## Retry Policy

The client automatically retries transient HTTP errors (5xx, 408) using exponential backoff:

- **Retry 1**: Wait 2 seconds
- **Retry 2**: Wait 4 seconds  
- **Retry 3**: Wait 8 seconds

## Error Handling

### Exception-Based (Default)

By default, Refit clients throw `ApiException` on HTTP errors:

```csharp
using Refit;

try
{
    var user = await _identityClient.RegisterUserAsync(dto);
}
catch (ApiException ex)
{
    Console.WriteLine($"API Error: {ex.StatusCode}");
    Console.WriteLine($"Content: {ex.Content}");
}
```

### Result-Based (Recommended)

Use `Safe*` extension methods for functional error handling with `FluentResults`:

```csharp
using Dilcore.WebApi.Client.Extensions;
using FluentResults;

// Returns Result<UserDto> instead of throwing exceptions
var result = await _identityClient.SafeRegisterUserAsync(dto);

if (result.IsSuccess)
{
    Console.WriteLine($"User registered: {result.Value.Email}");
}
else
{
    // Errors include ProblemDetails information
    var error = result.Errors.First() as ApiError;
    Console.WriteLine($"Error: {error.Message}");
    Console.WriteLine($"Status Code: {error.StatusCode}");
    Console.WriteLine($"Error Code: {error.Code}");
    Console.WriteLine($"Trace ID: {error.TraceId}");
}
```

### Available Safe Methods

All client methods have corresponding `Safe*` variants:

**Identity Client:**
- `SafeRegisterUserAsync()` - Register a new user
- `SafeGetCurrentUserAsync()` - Get current user profile

**Tenancy Client:**
- `SafeCreateTenantAsync()` - Create a new tenant
- `SafeGetTenantAsync()` - Get current tenant

### Error Types

`ApiError` extends `AppError` and includes:

| Property | Type | Description |
|----------|------|-------------|
| `StatusCode` | `int` | HTTP status code (400, 401, 404, etc.) |
| `Code` | `string` | Error code from ProblemDetails |
| `Message` | `string` | Error message |
| `Type` | `ErrorType` | Validation, NotFound, Unauthorized, etc. |
| `TraceId` | `string?` | Request trace ID for debugging |
| `Timestamp` | `DateTime?` | When the error occurred |
| `Extensions` | `Dictionary?` | Additional error metadata |

### Pattern Matching

```csharp
var result = await _identityClient.SafeGetCurrentUserAsync();

var message = result.IsSuccess
    ? $"Welcome, {result.Value.Email}"
    : result.Errors.First() switch
    {
        ApiError { StatusCode: 401 } => "Please log in",
        ApiError { StatusCode: 404 } => "User not found",
        ApiError error => $"Error: {error.Message}"
    };
```

## Development

### Building

```bash
dotnet build src/WebApi.Client/WebApi.Client.csproj
```

### Running Tests

```bash
# Unit tests
dotnet test tests/WebApi/WebApi.Client.Tests/WebApi.Client.Tests.csproj

# Architecture tests
dotnet test tests/WebApi/WebApi.Client.Architecture.Tests/WebApi.Client.Architecture.Tests.csproj
```
