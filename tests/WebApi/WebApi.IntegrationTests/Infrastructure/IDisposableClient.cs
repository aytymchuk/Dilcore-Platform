namespace Dilcore.WebApi.IntegrationTests.Infrastructure;

/// <summary>
/// Wrapper for a typed Refit client that ensures the underlying HttpClient is disposed.
/// </summary>
public interface IDisposableClient<out T> : IDisposable
{
    /// <summary>
    /// The typed Refit client.
    /// </summary>
    T Client { get; }
}
