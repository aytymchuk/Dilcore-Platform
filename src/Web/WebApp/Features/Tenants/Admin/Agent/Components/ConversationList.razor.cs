using Dilcore.WebApp.Models.Agent;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace Dilcore.WebApp.Features.Tenants.Admin.Agent.Components;

public partial class ConversationList
{
    private string _filter = string.Empty;

    [Parameter]
    public IReadOnlyList<ConversationSummary> Items { get; set; } = [];

    [Parameter]
    public string? ActiveThreadId { get; set; }

    [Parameter]
    public EventCallback<string> OnSelect { get; set; }

    [Parameter]
    public EventCallback OnNewChat { get; set; }

    private IEnumerable<ConversationSummary> FilteredItems
    {
        get
        {
            if (string.IsNullOrWhiteSpace(_filter))
            {
                return Items;
            }

            return Items.Where(i => i.Title.Contains(_filter.Trim(), StringComparison.OrdinalIgnoreCase));
        }
    }

    private static string GetItemClass(bool isActive) =>
        isActive ? "agent-convo__item agent-convo__item--active" : "agent-convo__item";

    private Task NotifyNewChatAsync(MouseEventArgs _) => OnNewChat.InvokeAsync();
}
