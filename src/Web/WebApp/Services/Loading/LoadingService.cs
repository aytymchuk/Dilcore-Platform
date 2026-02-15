using System.Collections.Concurrent;

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

    private void NotifyStateChanged() => OnChange?.Invoke();
}
