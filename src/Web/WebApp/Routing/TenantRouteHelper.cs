using Dilcore.WebApp.Constants;

namespace Dilcore.WebApp.Routing;

/// <summary>
/// Centralized helper for extracting tenant system names from URL paths.
/// Tenant routes follow the convention: /workspaces/{tenant}/...
/// </summary>
public static class TenantRouteHelper
{
    /// <summary>
    /// Extracts the tenant system name from a URL path.
    /// Returns null if the path is not a tenant-scoped route.
    /// </summary>
    public static string? ExtractTenantFromPath(string? path)
    {
        if (string.IsNullOrEmpty(path))
        {
            return null;
        }

        var segments = path.Split('/', StringSplitOptions.RemoveEmptyEntries);

        if (segments.Length < 2)
        {
            return null;
        }

        if (!string.Equals(segments[0], RouteConstants.Workspace.Prefix, StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        return segments[1];
    }
}
