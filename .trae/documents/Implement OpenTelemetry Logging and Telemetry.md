I will implement OpenTelemetry logging and telemetry in the `WebApi` project, enabling structured logging and version tracking.

### **1. Add Dependencies**
I will add the following NuGet packages to `src/WebApi/WebApi.csproj`:
*   `Azure.Monitor.OpenTelemetry.AspNetCore`: The official Azure Monitor Distro (includes ASP.NET Core, HttpClient, and SQL instrumentation).
*   `OpenTelemetry.Exporter.Console`: For structured console logging via OpenTelemetry.
*   `OpenTelemetry.Extensions.Hosting`: For `AddOpenTelemetry` extension methods.

### **2. Create Telemetry Configuration**
I will create `src/WebApi/Extensions/TelemetrySettings.cs` to bind the configuration:
```csharp
public class TelemetrySettings
{
    public string? ConnectionString { get; set; }
    public string ServiceName { get; set; } = "WebApi";
}
```

### **3. Create Telemetry Extensions**
I will create `src/WebApi/Extensions/TelemetryExtensions.cs` with an `AddTelemetry` extension method.
*   It will read the **Service Version** from the `BUILD_VERSION` environment variable, defaulting to `"local_development"` if null.
*   It will configure OpenTelemetry with this version.
*   **Logging**: It will always add `AddConsoleExporter()` to the OpenTelemetry logging provider.
*   **Azure Monitor**: It will check for the `ConnectionString`.
    *   **If present**: Call `UseAzureMonitor()` (enables Tracing, Metrics, and Logs export to Azure).
    *   **If absent**: Manually enable `AddAspNetCoreInstrumentation()` and `AddHttpClientInstrumentation()` so that tracing/metrics are still collected without sending to Azure.

### **4. Implement Structured Logging**
I will create `src/WebApi/Extensions/LoggerExtensions.cs` to define high-performance structured logging messages using `[LoggerMessage]`:
```csharp
internal static partial class LoggerExtensions
{
    [LoggerMessage(LogLevel.Information, "Getting weather forecast for {Count} days")]
    public static partial void LogGettingWeatherForecast(this ILogger logger, int count);
}
```

### **5. Update `Program.cs`**
I will modify `src/WebApi/Program.cs` to:
*   Call `builder.Services.AddTelemetry(builder.Configuration)`.
*   Inject `ILogger<Program>` into the `/weatherforecast` endpoint.
*   Call `logger.LogGettingWeatherForecast(5)` to demonstrate structured logging.

### **6. Update `appsettings.json`**
I will add the `Telemetry` configuration and tune the `Logging` section:
```json
"Telemetry": {
  "ServiceName": "Dilcore.WebApi"
  // "ConnectionString": "..." // Optional
},
"Logging": {
  "LogLevel": {
    "Default": "Information",
    "Microsoft.AspNetCore": "Warning"
  }
}
```

This setup fulfills all requirements including version tracking from `Dockerfile` environment variables.