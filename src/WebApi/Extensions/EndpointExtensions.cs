using Dilcore.Identity.WebApi;
using Dilcore.Tenancy.WebApi;

namespace Dilcore.WebApi.Extensions;

public static class EndpointExtensions
{
    public static WebApplication MapApplicationEndpoints(this WebApplication app)
    {
        // Domain Module Endpoints
        app.MapIdentityEndpoints();
        app.MapTenancyEndpoints();

        return app;
    }
}