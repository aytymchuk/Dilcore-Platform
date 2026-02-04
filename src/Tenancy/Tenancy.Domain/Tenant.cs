
using System.Text.RegularExpressions;
using Dilcore.Domain.Abstractions;

namespace Dilcore.Tenancy.Domain;

public sealed partial record Tenant : BaseDomain
{

    /// <summary>
    /// Prefix of the DB/Collection/Container name specified for this particular tenant.
    /// Cannot be changed after tenant creation.
    /// </summary>
    public required string StoragePrefix { get; init; }

    /// <summary>
    /// Unique system name (lower-kebab-case).
    /// </summary>
    public required string SystemName
    {
        get => _systemName;
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

            _systemName = value;
        }
    }

    private readonly string _systemName = string.Empty;

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
    public string CreatedById { get; init; } = string.Empty;

    [GeneratedRegex("[a-z0-9]+")]
    private static partial Regex KebabCasePartRegex();

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
}