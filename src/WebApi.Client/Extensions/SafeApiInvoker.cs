using Dilcore.WebApi.Client.Errors;
using FluentResults;
using Refit;

namespace Dilcore.WebApi.Client.Extensions;

/// <summary>
/// Internal helper for wrapping API calls in Result-based error handling.
/// </summary>
internal static class SafeApiInvoker
{
    /// <summary>
    /// Executes an API call and wraps the result in a Result type with comprehensive error handling.
    /// </summary>
    /// <typeparam name="T">The return type of the API call.</typeparam>
    /// <param name="apiCall">The API call to execute.</param>
    /// <returns>Result containing the API response or error information.</returns>
    internal static async Task<Result<T>> InvokeAsync<T>(Func<Task<T>> apiCall)
    {
        try
        {
            var result = await apiCall();
            return Result.Ok(result);
        }
        catch (ApiException apiEx)
        {
            var error = await ApiErrorHelper.ParseApiException(apiEx);
            return Result.Fail(error);
        }
        catch (HttpRequestException httpEx)
        {
            var error = ApiErrorHelper.CreateNetworkError(httpEx);
            return Result.Fail(error);
        }
        catch (TaskCanceledException ex)
        {
            var error = ex.CancellationToken.IsCancellationRequested
                ? ApiErrorHelper.CreateCancellationError()
                : ApiErrorHelper.CreateTimeoutError();
            return Result.Fail(error);
        }
        catch (Exception ex)
        {
            var error = ApiErrorHelper.CreateUnexpectedError(ex);
            return Result.Fail(error);
        }
    }
}
