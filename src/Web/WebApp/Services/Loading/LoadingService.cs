using System.Collections.Immutable;
using Dilcore.WebApp.Extensions;

namespace Dilcore.WebApp.Services.Loading;

public class LoadingService : ILoadingService
{
    private volatile ImmutableList<string> _loadingMessages = ImmutableList<string>.Empty;
    private readonly ILogger<LoadingService> _logger;

    public LoadingService(ILogger<LoadingService> logger)
    {
        _logger = logger;
    }

    public event Action? OnChange;

    public bool IsLoading => !_loadingMessages.IsEmpty;

    public string? CurrentMessage => _loadingMessages.LastOrDefault();
    public void Show(string message)
    {
        if (string.IsNullOrEmpty(message)) return;

#pragma warning disable CS0420
        ImmutableInterlocked.Update(ref _loadingMessages, list => list.Add(message));
#pragma warning restore CS0420
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
#pragma warning disable CS0420
            if (Interlocked.CompareExchange(ref _loadingMessages, newList, oldList) == oldList)
#pragma warning restore CS0420
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
            catch (Exception ex)
            {
                // Log exception from subscriber to prevent it from breaking other subscribers
                // or the loading service itself.
                _logger.LogLoadingSubscriberError(ex, d.Method.Name);
            }
        }
    }
}
