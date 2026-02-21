using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Dilcore.Blueprints.WebApi;

/// <summary>
/// HTTP endpoints for the Blueprints module.
/// </summary>
public static class EndpointExtensions
{
    /// <summary>
    /// Maps all Blueprints module endpoints.
    /// </summary>
    public static void MapBlueprintsEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/blueprints")
            .WithTags("Blueprints")
            .RequireAuthorization();

        // Map endpoints here
    }
}
