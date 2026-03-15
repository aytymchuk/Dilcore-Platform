using Dilcore.WebApp.Features.Tenants.Context;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;

namespace Dilcore.WebApp.Features.Tenants.Admin.Agent;

public record ChatMessage(string Content, bool IsUser, DateTime Timestamp);

public partial class AdminAgent
{
    [Inject]
    private IJSRuntime JSRuntime { get; set; } = default!;

    private readonly List<ChatMessage> _messages = new()
    {
        new ChatMessage(
            "I've updated the schema based on your request. I added a new `status` field with enum constraints and linked the `Customer` entity as a mandatory reference. Would you like me to generate the corresponding Projections as well?",
            IsUser: false,
            Timestamp: DateTime.UtcNow.AddMinutes(-2)),
        new ChatMessage(
            "Looks good. Please also add an 'AuditLog' trait to this entity so we can track changes to the order status automatically.",
            IsUser: true,
            Timestamp: DateTime.UtcNow.AddMinutes(-1))
    };

    private string _userInput = string.Empty;
    private bool _isTyping;
    private const string SchemaFileName = "OrderSystem.json";
    private const string SchemaPreviewContent = @"{
  ""entity"": ""Order"",
  ""fields"": [
    {
      ""name"": ""id"",
      ""type"": ""UUID"",
      ""primary"": true
    },
    {
      ""name"": ""status"",
      ""type"": ""Enum"",
      ""options"": [""PENDING"", ""PAID""]
    }
  ]
}";

    private async Task SendMessageAsync()
    {
        if (string.IsNullOrWhiteSpace(_userInput))
        {
            return;
        }

        var userMessage = new ChatMessage(_userInput.Trim(), IsUser: true, Timestamp: DateTime.UtcNow);
        _messages.Add(userMessage);
        _userInput = string.Empty;
        StateHasChanged();

        _isTyping = true;
        StateHasChanged();

        await Task.Delay(1500);

        _isTyping = false;
        _messages.Add(new ChatMessage(
            "I've added the `AuditLog` trait to the `Order` entity. Changes to order `status` will now be tracked automatically. Would you like me to show the updated schema or configure retention for the audit entries?",
            IsUser: false,
            Timestamp: DateTime.UtcNow));
        StateHasChanged();
    }

    private async Task CopySchemaToClipboardAsync()
    {
        try
        {
            await JSRuntime.InvokeVoidAsync("navigator.clipboard.writeText", SchemaPreviewContent);
        }
        catch
        {
            // Clipboard may be unavailable; no-op for this iteration
        }
    }

    private async Task HandleInputKeyDownAsync(KeyboardEventArgs e)
    {
        if (e.Key == "Enter" && !e.ShiftKey)
        {
            await SendMessageAsync();
        }
    }

    private string FormatTimestamp(DateTime timestamp)
    {
        var diff = DateTime.UtcNow - timestamp;
        if (diff.TotalSeconds < 60)
        {
            return "Sent just now";
        }

        if (diff.TotalMinutes < 60)
        {
            return $"{(int)diff.TotalMinutes} min ago";
        }

        return timestamp.ToString("g");
    }
}
