---
trigger: glob
globs: *.cs
---

# Logging Standards

## Overview
All logging in the codebase must use LoggerMessage source generator extension methods for performance, type safety, and maintainability.

## Rules

### 1. Never Use Direct Logger Calls
❌ **WRONG**:
```csharp
logger.LogInformation("Starting the application...");
logger.LogError(ex, "Error processing {Item}", itemName);
logger.LogWarning("Configuration {Config} is missing", configName);
```

✅ **CORRECT**:
```csharp
logger.LogStartingApplication();
logger.LogProcessingError(ex, itemName);
logger.LogConfigurationMissing(configName);
```

### 2. Define Extension Methods in LoggerExtensions.cs

Each project should have a `LoggerExtensions.cs` file containing all LoggerMessage extension methods.

**Location**: 
- WebApi: `src/WebApi/Extensions/LoggerExtensions.cs`
- MultiTenant: `src/Common/MultiTenant/MultiTenant.Http.Extensions/LoggerExtensions.cs`
- Other projects: `<ProjectRoot>/LoggerExtensions.cs`

**Template**:
```csharp
using Microsoft.Extensions.Logging;

namespace YourNamespace;

internal static partial class LoggerExtensions
{
    [LoggerMessage(LogLevel.Information, "Your message here")]
    public static partial void LogYourMethod(this ILogger logger);

    [LoggerMessage(LogLevel.Error, "Error message with {Parameter}")]
    public static partial void LogYourError(this ILogger logger, Exception ex, string parameter);
}
```

### 3. Naming Conventions

- **Method names**: Use `Log` prefix + descriptive action in PascalCase
  - Examples: `LogApplicationStarted`, `LogTenantResolved`, `LogAttributeProviderError`
- **Parameters**: Match the template placeholders exactly
  - Template: `"Error with {ProviderType}"` → Parameter: `string providerType`

### 4. Log Levels

Use appropriate log levels:
- **LogLevel.Debug**: Detailed diagnostic information (tenant resolution, internal state)
- **LogLevel.Information**: General informational messages (application lifecycle, major operations)
- **LogLevel.Warning**: Potentially harmful situations (missing config, degraded functionality)
- **LogLevel.Error**: Error events with exceptions (failed operations, caught exceptions)

### 5. Exception Handling

When logging exceptions, always include the exception parameter:

```csharp
[LoggerMessage(LogLevel.Error, "Error getting attributes from provider {ProviderType}")]
public static partial void LogAttributeProviderError(this ILogger logger, Exception ex, string providerType);
```

Usage:
```csharp
catch (Exception ex)
{
    _logger.LogAttributeProviderError(ex, provider.GetType().Name);
}
```

### 6. Organize by Feature

Group related log methods with comments:

```csharp
internal static partial class LoggerExtensions
{
    // Application Lifecycle
    [LoggerMessage(LogLevel.Information, "Application has started")]
    public static partial void LogApplicationStarted(this ILogger logger);

    // Tenant Resolution
    [LoggerMessage(LogLevel.Debug, "Tenant resolved by {Provider}: {TenantName}")]
    public static partial void LogTenantResolved(this ILogger logger, string provider, string tenantName);

    // Error Handling
    [LoggerMessage(LogLevel.Error, "Error processing {Operation}")]
    public static partial void LogOperationError(this ILogger logger, Exception ex, string operation);
}
```

## Benefits

1. **Performance**: Compile-time code generation eliminates runtime overhead
2. **Type Safety**: Compile-time validation of parameters
3. **Maintainability**: All log messages centralized in one file per project
4. **Consistency**: Standardized logging patterns across the codebase
5. **Refactoring**: Easy to update log messages without searching the entire codebase

## Migration Checklist

When adding new logging:
- [ ] Define extension method in `LoggerExtensions.cs`
- [ ] Use appropriate log level
- [ ] Include exception parameter for errors
- [ ] Use descriptive method name with `Log` prefix
- [ ] Test that the log message appears correctly

## Examples

### Simple Information Log
```csharp
// LoggerExtensions.cs
[LoggerMessage(LogLevel.Information, "Getting weather forecast for {Count} days")]
public static partial void LogGettingWeatherForecast(this ILogger logger, int count);

// Usage
logger.LogGettingWeatherForecast(5);
```

### Error with Exception
```csharp
// LoggerExtensions.cs
[LoggerMessage(LogLevel.Error, "Error getting attributes from provider {ProviderType}. Continuing with remaining providers.")]
public static partial void LogAttributeProviderError(this ILogger logger, Exception ex, string providerType);

// Usage
catch (Exception ex)
{
    _logger.LogAttributeProviderError(ex, provider.GetType().Name);
}
```

### Warning with Multiple Parameters
```csharp
// LoggerExtensions.cs
[LoggerMessage(LogLevel.Warning, "Authentication challenge issued for request {Path}: {Error} - {ErrorDescription}")]
public static partial void LogAuthenticationChallenge(this ILogger logger, string path, string error, string errorDescription);

// Usage
logger.LogAuthenticationChallenge(context.Request.Path, context.Error ?? "Unknown", context.ErrorDescription ?? "No description");
```
