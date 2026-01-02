# Validation Guide

This guide explains how validation works in the Dilcore Platform, covering both **runtime enforcement** using filters and **documentation generation** using OpenAPI.

## Runtime Enforcement

To enforce validation rules on an endpoint, use the `AddValidationFilter<T>()` extension method. This uses the `ValidationEndpointFilter` to automatically validate the request body against registered FluentValidation rules.

### Generic Component Usage

The `ValidationEndpointFilter` (located in `src/WebApi/Infrastructure/Validation/ValidationEndpointFilter.cs`) is a generic filter that:
1.  Resolves the `IValidator<T>` from the dependency injection container.
2.  Inspects the endpoint arguments to find the object of type `T`.
3.  Validates the object asynchronously.
4.  If invalid, returns a Problem Details response.

### How to Apply

Apply the filter to any endpoint builder (Minimal API):

```csharp
app.MapPost("/api/users", CreateUser)
   .AddValidationFilter<CreateUserDto>(); // Wraps endpoint with validation logic
```

This ensures that the `CreateUser` handler is only called if `CreateUserDto` is valid.

## OpenApi Documentation

The `OpenApiValidationSchemaTransformer` is a custom component that...

This ensures that the API documentation accurately reflects the validation logic enforced by the application, such as required fields, string lengths, and regex patterns.

## Registration

The transformer is registered as part of the OpenAPI configuration in `OpenApiExtensions.cs` using the `AddSchemaTransformer` method.

### Code Snippet (`src/WebApi/Infrastructure/OpenApi/OpenApiExtensions.cs`)

```csharp
public static IServiceCollection AddOpenApiDocumentation(this IServiceCollection services, IConfiguration configuration)
{
    // ...
    services.AddOpenApi(options =>
    {
        // ...
        // Add FluentValidation rules to schema
        options.AddSchemaTransformer<OpenApiValidationSchemaTransformer>();
    });
    // ...
}
```

This ensures it runs for every schema generation request.

## Transformation Logic

The transformer (`OpenApiValidationSchemaTransformer.cs`) inspects the FluentValidation validators associated with the DTO types used in your API. It maps specific validator types to OpenAPI schema properties.

| FluentValidation Rule | OpenAPI Schema Property | Description |
|-----------------------|-------------------------|-------------|
| `.NotEmpty()`, `.NotNull()` | `required: [...]` | Adds the property name to the parent schema's `required` list. |
| `.Length(min, max)` | `minLength`, `maxLength` | Sets string length constraints. |
| `.MinimumLength(n)` | `minLength` | Sets minimum string length. |
| `.MaximumLength(n)` | `maxLength` | Sets maximum string length. |
| `.InclusiveBetween(min, max)` | `minimum`, `maximum` | Sets numeric range constraints. |
| `.EmailAddress()` | `format: "email"` | Marks the string format as an email address. |
| `.Matches(regex)` | `pattern` | Extracts the regex pattern. |
| `IRegularExpressionValidator` | `pattern` | Extracts custom regex from any compatible validator. |


### Regex Extraction Details
For standard validators like `RegularExpressionValidator`, the regex pattern is directly extracted. For the `EmailValidator`, the transformer checks if it implements `IRegularExpressionValidator` (e.g., custom configuration) or uses a fallback reflection to check for an `Expression` property, ensuring even wrapped validators are supported.

## Validation Error Result

When a request fails validation, the API returns a **400 Bad Request** response using the standard [Problem Details for HTTP APIs (RFC 7807)](https://tools.ietf.org/html/rfc7807).

### Response Structure

The response includes:
-   **Standard Fields**: `type`, `title`, `status`, `detail`, `instance`.
-   **Extensions**:
  - `traceId`: For distributed tracing.
  - `errorCode`: A machine-readable code (e.g., `DATA_VALIDATION_FAILED`).
  - `errors`: A dictionary of field names and their corresponding error messages.

### Example Response

```json
{
  "type": "https://api.dilcore.com/errors/data-validation-failed",
  "title": "Data Validation Failed",
  "status": 400,
  "detail": "One or more validation errors occurred.",
  "instance": "/api/v1/auth/register",
  "extensions": {
    "traceId": "00-50d321591871216b3f71c480031804e1-207008fd3962d665-01",
    "errorCode": "DATA_VALIDATION_FAILED",
    "timestamp": "2026-01-02T00:35:00.123Z",
    "errors": {
      "email": [
        "Email is required.",
        "Email must be a valid email address."
      ],
      "password": [
        "Password must be at least 8 characters long."
      ]
    }
  }
}
```
