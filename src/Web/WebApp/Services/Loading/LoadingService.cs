namespace Dilcore.WebApp.Services.Loading;

public class LoadingService : ILoadingService
{
    private readonly List<string> _loadingMessages = new();
    private readonly object _lock = new();

    public event Action? OnChange;

    public bool IsLoading
    {
        get
        {
            lock (_lock)
            {
                return _loadingMessages.Count > 0;
            }
        }
    }

    public string? CurrentMessage
    {
        get
        {
            lock (_lock)
            {
                return _loadingMessages.LastOrDefault();
            }
        }
    }

    public void Show(string message)
    {
        lock (_lock)
        {
            _loadingMessages.Add(message);
        }
        NotifyStateChanged();
    }

    public void Hide(string message)
    {
        lock (_lock)
        {
            _loadingMessages.Remove(message);
        }
        NotifyStateChanged();
    }

    private void NotifyStateChanged()
    {
        var delegates = OnChange?.GetInvocationList();
        if (delegates == null) return;

        foreach (var d in delegates)
        {
            try
            {
                ((Action)d).Invoke();
            }
            catch
            {
                // Ignore exceptions from subscribers to prevent breaking other subscribers
                // or the loading service itself.
            }
        }
    }
}
