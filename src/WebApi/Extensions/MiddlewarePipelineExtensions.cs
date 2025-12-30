using Dilcore.WebApi.Infrastructure.OpenApi;
using Dilcore.WebApi.Infrastructure.Scalar;

namespace Dilcore.WebApi.Extensions;

public static class MiddlewarePipelineExtensions
{
    public static WebApplication UseApplicationMiddleware(this WebApplication app)
    {
        app.UseExceptionHandler();
        app.UseStatusCodePages();

        // Configure OpenAPI documentation in development
        if (app.Environment.IsDevelopment())
        {
            app.UseOpenApiDocumentation();
            app.AddScalarDocumentation(app.Configuration);
        }

        app.UseHttpsRedirection();
        app.UseAuthentication();
        app.UseAuthorization();

        return app;
    }
}