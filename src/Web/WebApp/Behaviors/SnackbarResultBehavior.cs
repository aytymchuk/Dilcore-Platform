using FluentResults;
using MediatR;
using MudBlazor;

namespace Dilcore.WebApp.Behaviors;

/// <summary>
/// MediatR pipeline behavior that displays snackbar errors for failed Result responses.
/// </summary>
/// <typeparam name="TRequest">The request type.</typeparam>
/// <typeparam name="TResponse">The response type (must be ResultBase).</typeparam>
internal sealed class SnackbarResultBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
    where TResponse : ResultBase
{
    private readonly ISnackbar _snackbar;

    public SnackbarResultBehavior(ISnackbar snackbar)
    {
        _snackbar = snackbar ?? throw new ArgumentNullException(nameof(snackbar));
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var response = await next();

        if (response.IsFailed)
        {
            foreach (var error in response.Errors)
            {
                _snackbar.Add(error.Message, Severity.Error);
            }
        }

        return response;
    }
}
