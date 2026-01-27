using Dilcore.WebApp.Constants;
using Microsoft.AspNetCore.Components;

namespace Dilcore.WebApp.Services;

public interface IAppNavigator
{
    void ToHome(bool forceLoad = false);
    void ToLogin();
    void ToLogout();
    void ToRegister();
}

public class AppNavigator : IAppNavigator
{
    private readonly NavigationManager _navigationManager;

    public AppNavigator(NavigationManager navigationManager)
    {
        _navigationManager = navigationManager;
    }

    public void ToHome(bool forceLoad = false) => _navigationManager.NavigateTo(RouteConstants.Home, forceLoad);

    public void ToLogin() => _navigationManager.NavigateTo(RouteConstants.Identity.Login, forceLoad: true);

    public void ToLogout() => _navigationManager.NavigateTo(RouteConstants.Identity.Logout, forceLoad: true);

    public void ToRegister() => _navigationManager.NavigateTo(RouteConstants.Users.Register);
}
