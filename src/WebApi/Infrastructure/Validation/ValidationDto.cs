namespace Dilcore.WebApi.Infrastructure.Validation;

/// <summary>
/// Test DTO with various field types for demonstrating validation.
/// </summary>
public sealed record ValidationDto
{
    /// <summary>
    /// Name of the person (required, 2-100 characters).
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Email address (required, valid email format).
    /// </summary>
    public required string Email { get; init; }

    /// <summary>
    /// Age in years (required, 0-150 range).
    /// </summary>
    public required int Age { get; init; }

    /// <summary>
    /// Phone number (optional, E.164 format).
    /// </summary>
    public string? PhoneNumber { get; init; }

    /// <summary>
    /// Website URL (optional, valid URL format).
    /// </summary>
    public string? Website { get; init; }

    /// <summary>
    /// Tags for categorization (optional, max 10 items, each max 50 chars).
    /// </summary>
    public string[]? Tags { get; init; }

    /// <summary>
    /// Start date (required).
    /// </summary>
    public required DateOnly StartDate { get; init; }

    /// <summary>
    /// End date (optional, must be after StartDate if provided).
    /// </summary>
    public DateOnly? EndDate { get; init; }
}
