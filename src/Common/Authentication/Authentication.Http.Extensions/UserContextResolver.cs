using Dilcore.Authentication.Abstractions;
using Dilcore.Authentication.Abstractions.Exceptions;
using Microsoft.Extensions.Logging;

namespace Dilcore.Authentication.Http.Extensions;

/// <summary>
/// Resolves user context lazily using registered providers.
/// Registered as a singleton for use in telemetry enrichment (OpenTelemetry) where
/// per-request scoped services are not available. The resolver safely accesses per-request
/// data by delegating to IUserContextProvider implementations, which use IHttpContextAccessor
/// to retrieve the current request's HttpContext. This design allows the singleton resolver
/// to be injected into singleton telemetry processors while still resolving user data
/// correctly for each request.
/// </summary>
public sealed class UserContextResolver : IUserContextResolver
{
    private readonly IEnumerable<IUserContextProvider> _providers;
    private readonly ILogger<UserContextResolver> _logger;

    public UserContextResolver(
        IEnumerable<IUserContextProvider> providers,
        ILogger<UserContextResolver> logger)
    {
        _providers = providers.OrderByDescending(p => p.Priority);
        _logger = logger;
    }

    public IUserContext Resolve()
    {
        if (TryResolve(out var userContext))
        {
            return userContext!;
        }

        _logger.LogNoUserResolved();
        throw new UserNotResolvedException("No user could be resolved from the current request.");
    }

    public bool TryResolve(out IUserContext? userContext)
    {
        foreach (var provider in _providers)
        {
            var context = provider.GetUserContext();
            if (context != null)
            {
                _logger.LogUserResolved(provider.GetType().Name, context.Id);
                userContext = context;
                return true;
            }
        }

        userContext = null;
        return false;
    }
}