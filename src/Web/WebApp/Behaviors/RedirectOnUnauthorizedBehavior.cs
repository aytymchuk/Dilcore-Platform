using Dilcore.WebApi.Client.Errors;
using Dilcore.WebApp.Constants;
using FluentResults;
using MediatR;
using Microsoft.AspNetCore.Components;

namespace Dilcore.WebApp.Behaviors;

/// <summary>
/// MediatR pipeline behavior that redirects to login page when an API result indicates unauthorized access (401).
/// </summary>
/// <typeparam name="TRequest">The request type.</typeparam>
/// <typeparam name="TResponse">The response type (must be ResultBase).</typeparam>
internal sealed class RedirectOnUnauthorizedBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
    where TResponse : ResultBase
{
    private readonly NavigationManager _navigationManager;

    public RedirectOnUnauthorizedBehavior(NavigationManager navigationManager)
    {
        _navigationManager = navigationManager ?? throw new ArgumentNullException(nameof(navigationManager));
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
                if (error is ApiError apiError && apiError.StatusCode == 401)
                {
                    // Force a hard reload to ensure a fresh authentication flow
                    _navigationManager.NavigateTo(RouteConstants.Identity.Login, forceLoad: true);
                    break;
                }
            }
        }

        return response;
    }
}
