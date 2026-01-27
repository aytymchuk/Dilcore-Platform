namespace Dilcore.WebApi.Client;

/// <summary>
/// Configuration options for the Platform API client.
/// </summary>
public class ApiSettings
{
    /// <summary>
    /// Gets or sets the base address of the Platform API.
    /// </summary>
    public Uri BaseUrl { get; set; } = null!;

    /// <summary>
    /// Gets or sets the HTTP request timeout.
    /// Default is 30 seconds.
    /// </summary>
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(90);

    /// <summary>
    /// Gets or sets the number of retry attempts for transient failures.
    /// Default is 3.
    /// </summary>
    public int RetryCount { get; set; } = 3;

    /// <summary>
    /// Gets or sets the base delay in seconds between retry attempts (exponential backoff).
    /// Default is 2 seconds.
    /// </summary>
    public double RetryDelaySeconds { get; set; } = 2.0;
}