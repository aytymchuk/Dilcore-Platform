using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Dilcore.WebApp.Components.Pages;

public partial class Home
{
    // IDialogService is injected via [Inject] attribute in the razor file or here.
    // Since we are moving towards code-behind, we can keep the @inject directive in .razor or move it here as a property.
    // For consistency with other refactorings, I will keep the @inject in .razor for now as there is no @code block to move. 
    // However, the partial class is needed for the compiler to merge them if we add logic later.
}
