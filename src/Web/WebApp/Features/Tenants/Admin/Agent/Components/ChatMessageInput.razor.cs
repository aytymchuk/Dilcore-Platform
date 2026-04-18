using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace Dilcore.WebApp.Features.Tenants.Admin.Agent.Components;

public partial class ChatMessageInput
{
    [Parameter]
    public string Value { get; set; } = string.Empty;

    [Parameter]
    public EventCallback<string> ValueChanged { get; set; }

    [Parameter]
    public string Placeholder { get; set; } = string.Empty;

    [Parameter]
    public bool Disabled { get; set; }

    [Parameter]
    public bool IsStreaming { get; set; }

    [Parameter]
    public EventCallback<string> OnSend { get; set; }

    private Task HandleValueChangedAsync(string value) => ValueChanged.InvokeAsync(value);

    private async Task HandleKeyDownAsync(KeyboardEventArgs e)
    {
        if (e.Key != "Enter" || e.ShiftKey)
        {
            return;
        }

        await SendAsync();
    }

    private async Task SendAsync()
    {
        var text = Value.Trim();
        if (string.IsNullOrEmpty(text) || Disabled || IsStreaming)
        {
            return;
        }

        await OnSend.InvokeAsync(text);
        await ValueChanged.InvokeAsync(string.Empty);
    }
}
