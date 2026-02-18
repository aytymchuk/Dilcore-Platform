using Dilcore.WebApp.Services.Loading;
using Microsoft.AspNetCore.Components;

namespace Dilcore.WebApp.Components.Common;

/// <summary>
/// Base component that provides thread-safe loading state management.
/// </summary>
public abstract class AsyncComponentBase : ComponentBase
{
    private int _busyCount;

    /// <summary>
    /// Indicates whether any async operation is currently in progress.
    /// Thread-safe property derived from an atomic counter.
    /// </summary>
    protected bool IsLoading => _busyCount > 0;

    /// <summary>
    /// Indicates whether the strict execute-async lock is active.
    /// </summary>
    protected bool IsBusy { get; set; }

    [Inject]
    public ILoadingService? LoadingService { get; set; }

    /// <summary>
    /// Executes an async action while tracking the busy state.
    /// </summary>
    protected async Task ExecuteBusyAsync(Func<Task> action)
    {
        try
        {
            var incrementResult = Interlocked.Increment(ref _busyCount);
            // Trigger UI update when ensuring busy state starts
            if (incrementResult == 1)
            {
                await InvokeAsync(StateHasChanged);
            }

            await action();
        }
        finally
        {
            var decrementResult = Interlocked.Decrement(ref _busyCount);
            // Trigger UI update when busy state ends
            if (decrementResult == 0)
            {
                await InvokeAsync(StateHasChanged);
            }
        }
    }

    /// <summary>
    /// Executes an async action while showing a loading screen with the specified text.
    /// </summary>
    protected async Task ExecuteAsync(Func<Task> action, string? loadingText = null)
    {
        if (IsBusy) return;

        var message = loadingText ?? "Loading...";

        try
        {
            IsBusy = true;
            if (LoadingService != null)
            {
                LoadingService.Show(message);
            }
            await action();
        }
        finally
        {
            if (LoadingService != null)
            {
                LoadingService.Hide(message);
            }
            IsBusy = false;
        }
    }
}
