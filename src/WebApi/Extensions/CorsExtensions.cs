using Microsoft.AspNetCore.HttpOverrides;

namespace Dilcore.WebApi.Extensions;

public static class CorsExtensions
{
    public static IServiceCollection AddCorsPolicy(this IServiceCollection services)
    {
        services.AddCors();
        services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
            options.ForwardLimit = 1;
            options.KnownProxies.Clear();
            options.KnownIPNetworks.Clear();
        });

        return services;
    }

    public static WebApplication UseCorsPolicy(this WebApplication app)
    {
        app.UseForwardedHeaders();
        app.UseCors(policy => policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());

        return app;
    }
}