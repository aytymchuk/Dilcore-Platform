namespace Dilcore.Configuration.AspNetCore;

/// <summary>
/// Constants for configuration-related functionality.
/// </summary>
public static class Constants
{
    /// <summary>
    /// The configuration key for shared settings.
    /// </summary>
    public const string SharedKey = "Shared";

    /// <summary>
    /// The configuration key for Azure App Configuration endpoint.
    /// </summary>
    public const string AppConfigEndpointKey = "AppConfigEndpoint";

    /// <summary>
    /// The environment variable key for build version.
    /// </summary>
    public const string BuildVersionKey = "BUILD_VERSION";

    /// <summary>
    /// The default build version used in local development.
    /// </summary>
    public const string DefaultBuildVersion = "local_development";
}