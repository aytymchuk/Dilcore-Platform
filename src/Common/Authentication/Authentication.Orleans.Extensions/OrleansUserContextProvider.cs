using Dilcore.Authentication.Abstractions;

namespace Dilcore.Authentication.Orleans.Extensions;

/// <summary>
/// Provides user context from Orleans RequestContext.
/// This provider has a higher priority (200) than the HTTP provider (100),
/// so it will be checked first in Orleans grain contexts.
/// </summary>
public sealed class OrleansUserContextProvider : IUserContextProvider
{
    /// <inheritdoc />
    public int Priority => 200;

    /// <inheritdoc />
    public IUserContext? GetUserContext()
    {
        return OrleansUserContextAccessor.GetUserContext();
    }
}
