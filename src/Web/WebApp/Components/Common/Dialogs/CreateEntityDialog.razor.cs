using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Dilcore.WebApp.Components.Common.Dialogs;

public partial class CreateEntityDialog
{
    [CascadingParameter] IMudDialogInstance MudDialog { get; set; } = default!;

    [Parameter] public string Title { get; set; } = "Create Entity";
    [Parameter] public string Subtitle { get; set; } = "Please fill in the details below.";
    [Parameter] public string Label { get; set; } = "New Entity";
    [Parameter] public string Icon { get; set; } = Icons.Material.Filled.Add;
    [Parameter] public string SubmitButtonText { get; set; } = "Create";
    [Parameter] public RenderFragment? ChildContent { get; set; }
    [Parameter] public EventCallback OnSubmit { get; set; }
    [Parameter] public bool IsProcessing { get; set; }

    private void Cancel() => MudDialog.Cancel();
    
    private async Task SubmitAsync()
    {
        if (OnSubmit.HasDelegate)
        {
            await OnSubmit.InvokeAsync();
        }
        else
        {
            MudDialog.Close(DialogResult.Ok(true));
        }
    }
}