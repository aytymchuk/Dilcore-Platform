using System.Collections.Immutable;

namespace Dilcore.WebApp.Services.Loading;

public class LoadingService : ILoadingService
{
    private ImmutableList<string> _loadingMessages = ImmutableList<string>.Empty;

    public event Action? OnChange;

    public bool IsLoading => !_loadingMessages.IsEmpty;

    public string? CurrentMessage => _loadingMessages.LastOrDefault();
    public void Show(string message)
    {
        if (string.IsNullOrEmpty(message)) return;

        ImmutableInterlocked.Update(ref _loadingMessages, list => list.Add(message));
        NotifyStateChanged();
    }

    public void Hide(string message)
    {
        // Optimistic concurrency loop to detect if removal actually happened
        var spinWait = new SpinWait();
        while (true)
        {
            var oldList = _loadingMessages;
            var newList = oldList.Remove(message);

            // If the list is unchanged (item not found), no update needed
            if (oldList == newList) return;

            // Attempt to atomically update the list
            if (Interlocked.CompareExchange(ref _loadingMessages, newList, oldList) == oldList)
            {
                // Successful update
                NotifyStateChanged();
                return;
            }

            // Update failed (concurrent modification), retry
            spinWait.SpinOnce();
        }
    }

    private void NotifyStateChanged()
    {
        var delegates = OnChange?.GetInvocationList();
        if (delegates == null) 
        {
            return;
        }

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
