using Dilcore.Telemetry.Abstractions;

namespace Dilcore.WebApi.Extensions;

/// <summary>
/// Provides user identity attributes for OpenTelemetry log records.
/// </summary>
public class UserAttributeProvider : ITelemetryAttributeProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UserAttributeProvider(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public IEnumerable<KeyValuePair<string, object?>> GetAttributes()
    {
        var context = _httpContextAccessor.HttpContext;
        if (context == null)
        {
            yield break;
        }

        var userId = context.GetUserId();
        yield return new KeyValuePair<string, object?>("user.id", userId);
    }
}