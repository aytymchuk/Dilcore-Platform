using Dilcore.Authentication.Abstractions;
using Dilcore.Telemetry.Abstractions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Dilcore.Authentication.Http.Extensions;

/// <summary>
/// Provides user attributes for OpenTelemetry telemetry enrichment.
/// </summary>
public sealed class UserAttributeProvider(IServiceProvider serviceProvider) : ITelemetryAttributeProvider
{
    public IEnumerable<KeyValuePair<string, object?>> GetAttributes()
    {
        var httpContextAccessor = serviceProvider.GetService<IHttpContextAccessor>();
        var context = httpContextAccessor?.HttpContext;
        if (context == null)
        {
            return [];
        }

        var userContextResolver = context.RequestServices.GetService<IUserContextResolver>();
        if (userContextResolver == null || !userContextResolver.TryResolve(out var userContext) || userContext?.Id == null)
        {
            return [];
        }

        return
        [
            new KeyValuePair<string, object?>("user.id", userContext.Id),
            new KeyValuePair<string, object?>("user.email", userContext.Email)
        ];
    }
}