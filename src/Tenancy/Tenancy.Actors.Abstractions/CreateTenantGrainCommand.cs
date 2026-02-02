
namespace Dilcore.Tenancy.Actors.Abstractions;

/// <summary>
/// Command to create a new tenant within the actor system.
/// </summary>
[GenerateSerializer]
public sealed record CreateTenantGrainCommand
{
    [Id(0)]
    public required string DisplayName { get; init; }

    [Id(1)]
    public string? Description { get; init; }
}
