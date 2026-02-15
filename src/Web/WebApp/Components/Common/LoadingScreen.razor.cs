using Dilcore.WebApp.Services.Loading;
using Microsoft.AspNetCore.Components;

namespace Dilcore.WebApp.Components.Common;

public partial class LoadingScreen : IDisposable
{
    [Inject]
    private ILoadingService LoadingService { get; set; } = null!;

    protected override void OnInitialized()
    {
        LoadingService.OnChange += StateHasChanged;
    }

    public void Dispose()
    {
        LoadingService.OnChange -= StateHasChanged;
    }
}
