using Finbuckle.MultiTenant.AspNetCore.Routing;
using Microsoft.AspNetCore.Http;

namespace Dilcore.MultiTenant.Http.Extensions.Extensions;

public static class HttpContextExtensions
{
    public static bool IsExcludedFromMultiTenant(this HttpContext context)
    {
        var endpoint = context.GetEndpoint();
        return endpoint == null || endpoint.Metadata.GetMetadata<IExcludeFromMultiTenantResolutionMetadata>() != null;
    }
}