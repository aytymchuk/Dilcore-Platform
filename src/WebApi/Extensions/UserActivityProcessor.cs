using System.Diagnostics;
using OpenTelemetry;

namespace Dilcore.WebApi.Extensions;

/// <summary>
/// Enriches OpenTelemetry activities (traces) with user context information.
/// </summary>
public class UserActivityProcessor : BaseProcessor<Activity>
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UserActivityProcessor(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public override void OnEnd(Activity data)
    {
        var context = _httpContextAccessor.HttpContext;
        if (context == null)
        {
            base.OnEnd(data);
            return;
        }

        // Extract User ID from User Identity
        var userId = context.User?.Identity?.Name ?? "anonymous";
        data.SetTag("user.id", userId);

        base.OnEnd(data);
    }
}
