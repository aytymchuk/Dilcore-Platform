namespace Dilcore.WebApi.Settings;

/// <summary>
/// Configuration settings for Orleans grain cluster.
/// </summary>
public class GrainsSettings
{
    /// <summary>
    /// Whether to use Azure Storage clustering. 
    /// Set to false for local development or testing with localhost clustering.
    /// </summary>
    public bool UseAzureClustering { get; set; } = true;

    /// <summary>
    /// Azure Storage account name for clustering.
    /// The connection is established via Managed Identity.
    /// </summary>
    public string StorageAccountName { get; set; } = string.Empty;

    /// <summary>
    /// Unique identifier for the Orleans cluster.
    /// </summary>
    public string ClusterId { get; set; } = "dilcore-cluster";

    /// <summary>
    /// Service identifier for the Orleans application.
    /// </summary>
    public string ServiceId { get; set; } = "dilcore-platform";
}
