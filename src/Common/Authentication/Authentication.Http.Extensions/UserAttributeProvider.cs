using Dilcore.Authentication.Abstractions;
using Dilcore.Telemetry.Abstractions;

namespace Dilcore.Authentication.Http.Extensions;

/// <summary>
/// Provides user attributes for OpenTelemetry telemetry enrichment.
/// </summary>
public sealed class UserAttributeProvider(IUserContextResolver userContextResolver) : ITelemetryAttributeProvider
{
    public IEnumerable<KeyValuePair<string, object?>> GetAttributes()
    {
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