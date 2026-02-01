
using System.Text.RegularExpressions;
using Dilcore.Domain.Abstractions;

namespace Dilcore.Tenancy.Domain;

public sealed record Tenant : BaseDomain
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
}
