namespace Dilcore.WebApp.Http.AiAgent;

/// <summary>
/// Configuration options for the AI Agent (blueprints-agent) API client.
/// </summary>
public class AgentApiSettings
{
    /// <summary>
    /// Gets or sets the base address of the Agent API.
    /// </summary>
    public Uri BaseUrl { get; set; } = null!;

    /// <summary>
    /// Gets or sets the HTTP request timeout for non-streaming calls.
    /// Default is 90 seconds.
    /// </summary>
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(90);

    /// <summary>
    /// Gets or sets the number of retry attempts for transient failures.
    /// Default is 3. (Not used by the current Agent client registration.)
    /// </summary>
    public int RetryCount { get; set; } = 3;

    /// <summary>
    /// Gets or sets the base delay in seconds between retry attempts (exponential backoff).
    /// Default is 2 seconds. (Not used by the current Agent client registration.)
    /// </summary>
    public double RetryDelaySeconds { get; set; } = 2.0;
}
