
using System.Text.RegularExpressions;
using Dilcore.Domain.Abstractions;

namespace Dilcore.Tenancy.Domain;

public sealed partial record Tenant : BaseDomain
{
    /// <summary>
    /// Unique identifier for the tenant's storage (DB/Collection/Container name).
    /// Cannot be changed after tenant creation.
    /// </summary>
    public required string StorageIdentifier { get; init; }

    /// <summary>
    /// Unique system name (lower-kebab-case).
    /// </summary>
    public required string SystemName
    {
        get;
        init
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException("System name must be in lower-kebab-case format.", nameof(value));
            }

            var parts = KebabCasePartRegex().Matches(value).Select(m => m.Value).ToList();
            if (parts.Count == 0 || string.Join("-", parts) != value)
            {
                throw new ArgumentException("System name must be in lower-kebab-case format.", nameof(value));
            }

            field = value;
        }
    } = string.Empty;

    /// <summary>
    /// Display name of the tenant.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Explanation of the tenant (used for AI context).
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// The user ID that created the tenant.
    /// </summary>
    public required string CreatedById { get; init; }

    /// <summary>
    /// Converts a display name to lower kebab-case.
    /// Example: "My New Tenant" -> "my-new-tenant"
    /// </summary>
    public static string ToKebabCase(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return string.Empty;
        }

        var parts = KebabCasePartRegex().Matches(input.ToLowerInvariant())
            .Select(m => m.Value);

        return string.Join("-", parts);
    }
    
    [GeneratedRegex("[a-z0-9]+")]
    private static partial Regex KebabCasePartRegex();
}