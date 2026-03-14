using System.Globalization;
using System.Text;
using Dilcore.WebApp.Constants;
using MudBlazor;

namespace Dilcore.WebApp.Routing;

/// <summary>
/// Builds breadcrumb items from the current URL path for workspace and admin layouts.
/// </summary>
public static class BreadcrumbBuilder
{
    private static readonly IReadOnlyDictionary<string, string> SegmentLabels = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        ["agent"] = "AI Agent",
        ["history"] = "History",
        ["entities"] = "Entities",
        ["projections"] = "Projections",
        ["forms"] = "Forms",
        ["views"] = "Views",
        ["workflows"] = "Workflows",
        ["settings"] = "Settings",
        ["integrations"] = "Integrations",
        ["team"] = "Team"
    };

    private const string RootLabel = "Dashboard";
    private const string WorkspaceRootLabel = "Workspace";
    private const string AdminRootLabel = "Administration";

    /// <summary>
    /// Builds breadcrumb items from the current path for the given tenant and layout type.
    /// </summary>
    /// <param name="currentUri">Current page URI (e.g. from NavigationManager.Uri).</param>
    /// <param name="tenant">Tenant system name from the URL.</param>
    /// <param name="isAdmin">True for admin layout, false for workspace layout.</param>
    /// <returns>List of breadcrumb items; never null. Last item is the current page (disabled).</returns>
    public static List<BreadcrumbItem> Build(string currentUri, string tenant, bool isAdmin)
    {
        if (string.IsNullOrEmpty(tenant))
        {
            return new List<BreadcrumbItem>
            {
                new BreadcrumbItem(isAdmin ? AdminRootLabel : WorkspaceRootLabel, href: null, disabled: true)
            };
        }

        var path = GetPathFromUri(currentUri);
        var segments = path.Split('/', StringSplitOptions.RemoveEmptyEntries);

        var prefixLength = isAdmin ? 3 : 2;
        if (segments.Length < prefixLength)
        {
            return BuildRootOnly(tenant, isAdmin);
        }

        if (!string.Equals(segments[0], RouteConstants.Workspace.Prefix, StringComparison.OrdinalIgnoreCase)
            || !string.Equals(segments[1], tenant, StringComparison.OrdinalIgnoreCase))
        {
            return BuildRootOnly(tenant, isAdmin);
        }

        if (isAdmin && !string.Equals(segments[2], RouteConstants.Workspace.Admin.Prefix, StringComparison.OrdinalIgnoreCase))
        {
            return BuildRootOnly(tenant, isAdmin);
        }

        var rootHref = isAdmin ? RouteConstants.Workspace.Admin.ForTenant(tenant) : RouteConstants.Workspace.ForTenant(tenant);
        var rootLabel = isAdmin ? AdminRootLabel : WorkspaceRootLabel;
        var trailStart = prefixLength;
        var trailLength = segments.Length - trailStart;

        if (trailLength == 0)
        {
            return new List<BreadcrumbItem>
            {
                new BreadcrumbItem(rootLabel, href: rootHref),
                new BreadcrumbItem(RootLabel, href: null, disabled: true)
            };
        }

        var result = new List<BreadcrumbItem> { new BreadcrumbItem(rootLabel, href: rootHref) };
        var pathBuilder = new StringBuilder(rootHref.TrimEnd('/'));

        for (var i = 0; i < trailLength; i++)
        {
            var segment = segments[trailStart + i];
            pathBuilder.Append('/').Append(segment);
            var href = pathBuilder.ToString();
            var label = GetSegmentLabel(segment);
            var isLast = i == trailLength - 1;

            result.Add(new BreadcrumbItem(label, href: isLast ? null : href, disabled: isLast));
        }

        return result;
    }

    private static List<BreadcrumbItem> BuildRootOnly(string tenant, bool isAdmin)
    {
        var rootHref = isAdmin ? RouteConstants.Workspace.Admin.ForTenant(tenant) : RouteConstants.Workspace.ForTenant(tenant);
        var rootLabel = isAdmin ? AdminRootLabel : WorkspaceRootLabel;

        return new List<BreadcrumbItem>
        {
            new BreadcrumbItem(rootLabel, href: rootHref),
            new BreadcrumbItem(RootLabel, href: null, disabled: true)
        };
    }

    private static string GetPathFromUri(string uri)
    {
        if (string.IsNullOrEmpty(uri))
        {
            return string.Empty;
        }

        var queryIndex = uri.IndexOf('?');
        var path = queryIndex >= 0 ? uri[..queryIndex] : uri;
        var hashIndex = path.IndexOf('#');

        if (hashIndex >= 0)
        {
            path = path[..hashIndex];
        }

        return path;
    }

    private static string GetSegmentLabel(string segment)
    {
        if (SegmentLabels.TryGetValue(segment, out var label))
        {
            return label;
        }

        var replaced = segment.Replace("-", " ", StringComparison.Ordinal);
        return CultureInfo.InvariantCulture.TextInfo.ToTitleCase(replaced.ToLowerInvariant());
    }
}
