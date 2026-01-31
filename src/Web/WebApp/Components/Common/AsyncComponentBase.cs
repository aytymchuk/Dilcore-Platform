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
    /// Executes an async action while tracking the busy state.
    /// </summary>
    protected async Task ExecuteBusyAsync(Func<Task> action)
    {
        try
        {
            Interlocked.Increment(ref _busyCount);
            // Trigger UI update when ensuring busy state starts
            if (_busyCount == 1)
            {
                await InvokeAsync(StateHasChanged);
            }

            await action();
        }
        finally
        {
            Interlocked.Decrement(ref _busyCount);
            // Trigger UI update when busy state ends
            if (_busyCount == 0)
            {
                await InvokeAsync(StateHasChanged);
            }
        }
    }
}
