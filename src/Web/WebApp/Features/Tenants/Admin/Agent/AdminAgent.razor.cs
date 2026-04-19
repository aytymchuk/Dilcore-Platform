using Dilcore.WebApp.Constants;
using Dilcore.WebApp.Components.Common;
using Dilcore.WebApp.Features.Tenants.Admin.Agent.Queries;
using Dilcore.WebApp.Http.AiAgent;
using Dilcore.WebApp.Http.AiAgent.Dtos;
using Dilcore.WebApp.Http.AiAgent.Streaming;
using Dilcore.WebApp.Models.Agent;
using Dilcore.WebApp.Services.Agent;

using MediatR;

using Microsoft.AspNetCore.Components;

using MudBlazor;

namespace Dilcore.WebApp.Features.Tenants.Admin.Agent;

public partial class AdminAgent : TenantComponentBase
{
    private readonly List<AgentQuickAction> _quickActions = BuildQuickActions();

    [Inject]
    private IBlueprintsAgentService AgentService { get; set; } = null!;

    [Inject]
    private ISender Sender { get; set; } = null!;

    [Inject]
    private IConversationTitleFactory TitleFactory { get; set; } = null!;

    [Inject]
    private NavigationManager Navigation { get; set; } = null!;

    [Inject]
    private ISnackbar Snackbar { get; set; } = null!;

    [Parameter]
    public string? ThreadId { get; set; }

    private List<ConversationSummary> _summaries = [];
    private List<ChatMessage> _messages = [];
    private string? _loadedThreadsTenant;
    private string? _previousRouteThreadId;
    private string? _loadedMessagesForThread;
    private bool _isLoadingThreads;
    private bool _isLoadingMessages;
    private string _inputText = string.Empty;
    private string? _pendingAssistant;
    private string? _pendingReasoning;
    private string? _finalAgentType;
    private string? _status;
    private bool _isStreaming;
    private string? _interruptThreadId;
    private CancellationTokenSource? _sendCts;
    private DateTime _lastStreamUi = DateTime.MinValue;

    private bool ShowQuickActions =>
        string.IsNullOrEmpty(ThreadId) && _messages.Count == 0 && !_isStreaming;

    private string? CurrentThreadTitle =>
        string.IsNullOrEmpty(ThreadId)
            ? null
            : _summaries.FirstOrDefault(s => s.Id == ThreadId)?.Title
              ?? (ThreadId.Length >= 8 ? $"Thread {ThreadId[..8]}" : $"Thread {ThreadId}");

    protected override async Task OnParametersSetAsync()
    {
        if (_loadedThreadsTenant != TenantSystemName)
        {
            _loadedThreadsTenant = TenantSystemName;
            _loadedMessagesForThread = null;
            _previousRouteThreadId = null;
            _messages.Clear();
            _summaries.Clear();
            _isLoadingThreads = true;
            await ExecuteBusyAsync(LoadThreadsAsync);
            _isLoadingThreads = false;
            StateHasChanged();
        }

        if (string.IsNullOrEmpty(ThreadId))
        {
            if (!string.IsNullOrEmpty(_previousRouteThreadId))
            {
                _messages.Clear();
                _loadedMessagesForThread = null;
            }

            _previousRouteThreadId = null;
        }
        else
        {
            if (_loadedMessagesForThread != ThreadId)
            {
                _loadedMessagesForThread = ThreadId;
                _isLoadingMessages = true;
                await ExecuteBusyAsync(() => LoadThreadMessagesAsync(ThreadId));
                _isLoadingMessages = false;
                StateHasChanged();
            }

            _previousRouteThreadId = ThreadId;
        }
    }

    public override async ValueTask DisposeAsync()
    {
        try
        {
            _sendCts?.Cancel();
            _sendCts?.Dispose();
        }
        finally
        {
            await base.DisposeAsync();
        }
    }

    private string GetRootClass() =>
        _summaries.Count == 0 && !_isLoadingThreads ? "agent-hub agent-hub--empty" : "agent-hub";

    private async Task LoadThreadsAsync()
    {
        var result = await Sender.Send(new GetAgentThreadsQuery());
        if (result.IsFailed)
        {
            return;
        }

        _summaries = result.Value
            .Select(t => new ConversationSummary(t.Id, TitleFactory.BuildTitle(t), MessageCount: 0))
            .ToList();
    }

    private async Task LoadThreadMessagesAsync(string id)
    {
        var result = await Sender.Send(new GetAgentThreadQuery(id));
        if (result.IsFailed)
        {
            return;
        }

        _messages = MapThreadToMessages(result.Value);
    }

    private static List<ChatMessage> MapThreadToMessages(ThreadStateDto thread)
    {
        var visible = thread.Messages.Where(m => !IsToolMessageType(m.Type)).ToList();
        var list = new List<ChatMessage>(visible.Count);
        for (var i = 0; i < visible.Count; i++)
        {
            var m = visible[i];
            var author = IsUserMessageType(m.Type)
                ? ChatAuthor.User
                : ChatAuthor.Assistant;
            var ts = DateTime.UtcNow.AddSeconds(-(visible.Count - i));
            list.Add(new ChatMessage(Guid.NewGuid(), author, m.Content, ts));
        }

        return list;
    }

    private static bool IsUserMessageType(string type) =>
        string.Equals(type, "user", StringComparison.OrdinalIgnoreCase)
        || string.Equals(type, "human", StringComparison.OrdinalIgnoreCase);

    private static bool IsToolMessageType(string type) =>
        string.Equals(type, "tool", StringComparison.OrdinalIgnoreCase);

    private static MessageDto? FindLastAiMessage(IReadOnlyList<MessageDto>? messages)
    {
        if (messages is null || messages.Count == 0)
        {
            return null;
        }

        for (var i = messages.Count - 1; i >= 0; i--)
        {
            var m = messages[i];
            if (string.Equals(m.Type, "ai", StringComparison.OrdinalIgnoreCase))
            {
                return m;
            }
        }

        return null;
    }

    private async Task SendAsync(string text)
    {
        if (_isStreaming)
        {
            return;
        }

        _sendCts?.Cancel();
        _sendCts?.Dispose();
        _sendCts = new CancellationTokenSource();
        var ct = _sendCts.Token;
        var beforeIds = _summaries.Select(s => s.Id).ToHashSet(StringComparer.Ordinal);

        try
        {
            _isStreaming = true;
            _status = null;
            _pendingAssistant = null;
            _pendingReasoning = null;
            _finalAgentType = null;
            _interruptThreadId = null;
            _lastStreamUi = DateTime.MinValue;

            _messages.Add(new ChatMessage(Guid.NewGuid(), ChatAuthor.User, text, DateTime.UtcNow));
            await InvokeAsync(StateHasChanged);

            var request = new ThreadMessageInputDto { Message = text };
            IAsyncEnumerable<BlueprintsAgentStreamEvent> stream = string.IsNullOrEmpty(ThreadId)
                ? AgentService.StartStreamAsync(request, ct)
                : AgentService.ContinueStreamAsync(ThreadId, request, ct);

            await foreach (var evt in stream.WithCancellation(ct))
            {
                switch (evt)
                {
                    case DeltaStreamEvent d when string.IsNullOrEmpty(d.AgentType):
                        _pendingReasoning = (_pendingReasoning ?? string.Empty) + (d.Content ?? string.Empty);
                        await MaybeRefreshStreamUiAsync();
                        break;
                    case DeltaStreamEvent d:
                        _pendingAssistant = (_pendingAssistant ?? string.Empty) + (d.Content ?? string.Empty);
                        if (!string.IsNullOrEmpty(d.AgentType))
                        {
                            _finalAgentType = d.AgentType;
                        }

                        await MaybeRefreshStreamUiAsync();
                        break;
                    case StatusStreamEvent s:
                        _status = string.Join(
                            " — ",
                            new[] { s.Message, s.Phase }.Where(x => !string.IsNullOrWhiteSpace(x)));
                        await InvokeAsync(StateHasChanged);
                        break;
                    case DataStreamEvent data:
                        if (string.IsNullOrEmpty(_interruptThreadId) && !string.IsNullOrEmpty(data.ThreadId))
                        {
                            _interruptThreadId = data.ThreadId;
                        }

                        var lastAi = FindLastAiMessage(data.Messages);
                        if (lastAi is not null)
                        {
                            _pendingAssistant = lastAi.Content;
                        }

                        await MaybeRefreshStreamUiAsync();
                        break;
                    case ThinkingStreamEvent t:
                        _status = string.IsNullOrWhiteSpace(t.Text)
                            ? AgentConstants.StreamingStatusThinking
                            : $"{AgentConstants.StreamingStatusThinking} {t.Text}";
                        await InvokeAsync(StateHasChanged);
                        break;
                    case InterruptStreamEvent i:
                        if (!string.IsNullOrEmpty(i.ThreadId))
                        {
                            _interruptThreadId = i.ThreadId;
                        }

                        _messages.Add(new ChatMessage(
                            Guid.NewGuid(),
                            ChatAuthor.Status,
                            string.IsNullOrWhiteSpace(i.Reason) ? AgentConstants.InterruptNotice : i.Reason!,
                            DateTime.UtcNow));
                        await InvokeAsync(StateHasChanged);
                        break;
                }
            }

            if (!string.IsNullOrWhiteSpace(_pendingAssistant) || !string.IsNullOrWhiteSpace(_pendingReasoning))
            {
                _messages.Add(new ChatMessage(
                    Guid.NewGuid(),
                    ChatAuthor.Assistant,
                    _pendingAssistant ?? string.Empty,
                    DateTime.UtcNow)
                {
                    Reasoning = string.IsNullOrWhiteSpace(_pendingReasoning) ? null : _pendingReasoning
                });
            }

            _pendingAssistant = null;
            _pendingReasoning = null;
            _finalAgentType = null;
            _status = null;
            _isStreaming = false;

            if (string.IsNullOrEmpty(ThreadId))
            {
                await LoadThreadsAsync();
                var newThread = _summaries.FirstOrDefault(s => !beforeIds.Contains(s.Id));
                var targetId = newThread?.Id ?? _interruptThreadId;
                if (!string.IsNullOrEmpty(targetId))
                {
                    Navigation.NavigateTo($"/workspaces/{TenantSystemName}/admin/agent/{Uri.EscapeDataString(targetId)}");
                }
            }

            await InvokeAsync(StateHasChanged);
        }
        catch (OperationCanceledException)
        {
            _isStreaming = false;
            _pendingAssistant = null;
            _pendingReasoning = null;
            _finalAgentType = null;
        }
        catch (Exception)
        {
            _isStreaming = false;
            _pendingAssistant = null;
            _pendingReasoning = null;
            _finalAgentType = null;
            Snackbar.Add(AgentConstants.SendErrorMessage, Severity.Error);
        }
    }

    private Task MaybeRefreshStreamUiAsync()
    {
        var n = DateTime.UtcNow;
        if ((n - _lastStreamUi).TotalMilliseconds < 50)
        {
            return Task.CompletedTask;
        }

        _lastStreamUi = n;
        return InvokeAsync(StateHasChanged);
    }

    private void OnQuickAction(AgentQuickAction action)
    {
        _inputText = action.Prompt;
    }

    private Task NewChatAsync()
    {
        Navigation.NavigateTo($"/workspaces/{TenantSystemName}/admin/agent");
        return Task.CompletedTask;
    }

    private Task SelectThreadAsync(string id)
    {
        Navigation.NavigateTo($"/workspaces/{TenantSystemName}/admin/agent/{Uri.EscapeDataString(id)}");
        return Task.CompletedTask;
    }

    private static List<AgentQuickAction> BuildQuickActions()
    {
        return
        [
            new AgentQuickAction(
                Icons.Material.Filled.Schema,
                AgentConstants.QuickActions.BuildDataSchemaTitle,
                AgentConstants.QuickActions.BuildDataSchemaDescription,
                AgentConstants.QuickActions.BuildDataSchemaPrompt),

            new AgentQuickAction(
                Icons.Material.Filled.AutoFixHigh,
                AgentConstants.QuickActions.AutomateLogicTitle,
                AgentConstants.QuickActions.AutomateLogicDescription,
                AgentConstants.QuickActions.AutomateLogicPrompt),

            new AgentQuickAction(
                Icons.Material.Filled.Speed,
                AgentConstants.QuickActions.PerformanceAuditTitle,
                AgentConstants.QuickActions.PerformanceAuditDescription,
                AgentConstants.QuickActions.PerformanceAuditPrompt),

            new AgentQuickAction(
                Icons.Material.Filled.MenuBook,
                AgentConstants.QuickActions.SystemGuideTitle,
                AgentConstants.QuickActions.SystemGuideDescription,
                AgentConstants.QuickActions.SystemGuidePrompt)
        ];
    }
}
