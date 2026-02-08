using Microsoft.AspNetCore.Components;

namespace Dilcore.WebApp.Components.Pages;

public partial class Counter
{
    private int currentCount = 0;

    private void IncrementCount()
    {
        currentCount++;
    }
}
