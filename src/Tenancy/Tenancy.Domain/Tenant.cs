
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

    private static readonly Regex KebabCaseRegex = new("^[a-z0-9]+(?:-[a-z0-9]+)*$", RegexOptions.Compiled);

    /// <summary>
    /// Unique system name (lower-kebab-case).
    /// </summary>
    public required string SystemName
    {
        get => _systemName;
        init
        {
            if (!KebabCaseRegex.IsMatch(value))
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
    /// Converts a display name to lower kebab-case.
    /// Example: "My New Tenant" -> "my-new-tenant"
    /// </summary>
    public static string ToKebabCase(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return string.Empty;
        }

        // Replace non-alphanumeric with spaces, then handle casing
        var normalized = NormalizationRegex().Replace(input.Trim(), " ");
        var parts = normalized.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return string.Join("-", parts).ToLowerInvariant();
    }

    [GeneratedRegex(@"[^a-zA-Z0-9\s]")]
    private static partial Regex NormalizationRegex();
}
