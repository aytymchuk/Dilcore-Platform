using Dilcore.Authentication.Abstractions;
using Dilcore.Telemetry.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace Dilcore.Authentication.Http.Extensions;

/// <summary>
/// Provides user attributes for OpenTelemetry telemetry enrichment.
/// </summary>
public sealed class UserAttributeProvider(IServiceProvider serviceProvider) : ITelemetryAttributeProvider
{
    public IEnumerable<KeyValuePair<string, object?>> GetAttributes()
    {
        var userContextResolver = serviceProvider.GetRequiredService<IUserContextResolver>();
        if (!userContextResolver.TryResolve(out var userContext) || userContext?.Id == null)
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