using Auth0.AspNetCore.Authentication;
using Dilcore.WebApp.Constants;
using Dilcore.WebApp.Helpers;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace Dilcore.WebApp.Extensions;

public static class WebApplicationExtensions
{
    public static WebApplication ConfigureWebApp(this WebApplication app)
    {
        app.UseForwardedHeaders();

        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Error", createScopeForErrors: true);
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseAntiforgery();
        app.MapStaticAssets();

        app.UseAuthentication();
        app.UseAuthorization();

        app.UseAuth0AuthenticationEndpoints();

        app.MapRazorComponents<App>()
            .AddInteractiveServerRenderMode();

        return app;
    }

    public static WebApplication UseAuth0AuthenticationEndpoints(this WebApplication app)
    {
        app.MapGet(RouteConstants.Identity.Login, async (HttpContext httpContext, string returnUrl = "/") =>
        {
            if (!UrlHelper.IsLocalUrl(returnUrl))
            {
                returnUrl = "/";
            }
            else if (returnUrl.StartsWith("~/"))
            {
                returnUrl = "/" + returnUrl.Substring(2);
            }

            var authenticationProperties = new LoginAuthenticationPropertiesBuilder()
                .WithRedirectUri(returnUrl)
                .Build();

            await httpContext.ChallengeAsync(Auth0Constants.AuthenticationScheme, authenticationProperties);
        });

        app.MapGet(RouteConstants.Identity.Logout, async httpContext =>
        {
            var authenticationProperties = new LogoutAuthenticationPropertiesBuilder()
                .WithRedirectUri("/")
                .Build();

            await httpContext.SignOutAsync(Auth0Constants.AuthenticationScheme, authenticationProperties);
            await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        });

        return app;
    }
}
