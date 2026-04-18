namespace Dilcore.WebApp.Services.Tenancy;

/// <summary>
/// Holds the current Blazor circuit's <see cref="IServiceProvider"/> for the duration of an inbound activity,
/// so code outside the normal component scope (e.g. <see cref="DelegatingHandler"/>) can resolve circuit-scoped services.
/// </summary>
public sealed class CircuitServicesAccessor
{
    private static readonly AsyncLocal<IServiceProvider?> Current = new();

    public IServiceProvider? Services
    {
        get => Current.Value;
        set => Current.Value = value;
    }
}
