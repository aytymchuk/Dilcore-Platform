namespace Dilcore.WebApp.Services.Loading;

/// <summary>
/// Service to manage global loading states.
/// </summary>
public interface ILoadingService
{
    /// <summary>
    /// Event triggered when the loading state changes.
    /// </summary>
    event Action OnChange;

    /// <summary>
    /// Checks if there are any active loading processes.
    /// </summary>
    bool IsLoading { get; }

    /// <summary>
    /// Gets the current loading message to display.
    /// </summary>
    string? CurrentMessage { get; }

    /// <summary>
    /// Shows the loading screen with the specified message.
    /// </summary>
    /// <param name="message">The message to display.</param>
    void Show(string message);

    /// <summary>
    /// Hides the loading screen for the specified message.
    /// </summary>
    /// <param name="message">The message that was displayed.</param>
    void Hide(string message);
}
