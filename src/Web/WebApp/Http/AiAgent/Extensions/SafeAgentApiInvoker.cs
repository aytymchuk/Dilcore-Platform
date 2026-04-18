using Dilcore.WebApi.Client.Errors;
using FluentResults;
using Refit;

namespace Dilcore.WebApp.Http.AiAgent.Extensions;

/// <summary>
/// Wraps blueprint agent Refit calls in <see cref="Result{T}"/> with the same error mapping as platform API clients.
/// </summary>
internal static class SafeAgentApiInvoker
{
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
