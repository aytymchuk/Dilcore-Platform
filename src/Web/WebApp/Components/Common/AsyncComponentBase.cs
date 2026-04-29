using Dilcore.WebApp.Services.Loading;
using Microsoft.AspNetCore.Components;
using Dilcore.WebApp.Extensions;

namespace Dilcore.WebApp.Components.Common;

/// <summary>
/// Base component that provides thread-safe loading state management.
/// </summary>
public abstract class AsyncComponentBase : ComponentBase, IAsyncDisposable
{
    private int _busyCount;
    private string? _activeLoadingMessage;

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

    [Inject]
    public ILogger<AsyncComponentBase>? ComponentLogger { get; set; }

    /// <summary>
    /// Executes an async action while tracking the busy state.
    /// </summary>
    protected async Task ExecuteBusyAsync(Func<Task> action)
    {
        var component = GetType().Name;

        ComponentLogger?.LogComponentOperationStarted(component, "ExecuteBusyAsync", null);

        try
        {
            var incrementResult = Interlocked.Increment(ref _busyCount);
            // Trigger UI update when ensuring busy state starts
            if (incrementResult == 1)
            {
                await InvokeAsync(StateHasChanged);
            }

            await action();

            ComponentLogger?.LogComponentOperationSucceeded(component, "ExecuteBusyAsync");
        }
        catch (Exception ex)
        {
            ComponentLogger?.LogComponentOperationException(ex, component, "ExecuteBusyAsync", null);
            throw;
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
        if (IsBusy)
        {
            return;
        }

        var component = GetType().Name;
        var message = loadingText ?? AsyncComponentConstants.DefaultLoadingMessage;

        ComponentLogger?.LogComponentOperationStarted(component, "ExecuteAsync", message);

        try
        {
            IsBusy = true;
            _activeLoadingMessage = message;

            LoadingService?.Show(message);

            await action();

            ComponentLogger?.LogComponentOperationSucceeded(component, "ExecuteAsync");
        }
        catch (Exception ex)
        {
            ComponentLogger?.LogComponentOperationException(ex, component, "ExecuteAsync", message);
            throw;
        }
        finally
        {
            LoadingService?.Hide(message);

            IsBusy = false;
            _activeLoadingMessage = null;
            StateHasChanged();
        }
    }

    public virtual ValueTask DisposeAsync()
    {
        if (_activeLoadingMessage is not null)
        {
            LoadingService?.Hide(_activeLoadingMessage);
            _activeLoadingMessage = null;
        }

        return ValueTask.CompletedTask;
    }
}
