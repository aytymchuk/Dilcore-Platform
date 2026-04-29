namespace Dilcore.WebApp.Constants;

public static class AgentConstants
{
    public const string PageTitle = "AI Agent";
    public const string HeroPrefix = "How can I assist your";
    public const string HeroHighlight = "Project";
    public const string HeroSuffix = "today?";
    public const string HeroSubtitle = "Select a quick action or start typing below to initialize a new agent workflow.";
    public const string InputPlaceholder = "Message AI Agent...";
    public const string FooterDisclaimer =
        "LLMs can make mistakes. Double-check important results.";
    public const string NewChatLabel = "New chat";
    public const string ConversationSearchPlaceholder = "Search history…";
    public const string StreamingStatusThinking = "Thinking…";
    public const string ReasoningLabel = "Reasoning";
    public const string ReasoningDecisionLabel = "Decision";
    public const string EmptyHistoryCaption = "Messages will appear here.";
    public const string InterruptNotice = "The agent is waiting for your input.";
    public const string ActiveThreadBadge = "Active";
    public const string SendErrorMessage = "Could not send your message. Please try again.";

    public static class QuickActions
    {
        public const string AgentGuideTitle = "Agent Guide";
        public const string AgentGuideDescription = "See what the agent can design, generate, and refine inside your workspace.";
        public const string AgentGuidePrompt = "Explain what functions this agent provides and how I can use it in this workspace.";

        public const string PersonalAccountingTitle = "Design Personal Accounting System";
        public const string PersonalAccountingDescription = "Model accounts, transactions, budgets, categories, and reporting flows.";
        public const string PersonalAccountingPrompt = "Help me design a personal accounting system for tracking accounts, budgets, expenses, and reports.";

        public const string CustomerCrmTitle = "Design Customer CRM";
        public const string CustomerCrmDescription = "Plan contacts, companies, deals, activity history, and sales pipeline stages.";
        public const string CustomerCrmPrompt = "Help me design a CRM system with customers, companies, deals, activities, and pipeline stages.";

        public const string InventoryWorkflowTitle = "Design Inventory Workflow";
        public const string InventoryWorkflowDescription = "Shape products, stock movements, purchase orders, suppliers, and alerts.";
        public const string InventoryWorkflowPrompt = "Help me design an inventory management workflow with products, suppliers, stock movements, and reorder alerts.";
    }

    public static class TestIds
    {
        public const string HeroSection = "agent-hero-section";
        public const string QuickActionsGrid = "agent-quick-actions-grid";
        public const string QuickActionCard = "agent-quick-action-card";
        public const string MessageInput = "agent-message-input";
        public const string SendButton = "agent-send-button";
        public const string ConversationList = "agent-conversation-list";
        public const string ConversationItem = "agent-conversation-item";
        public const string ChatHistory = "agent-chat-history";
        public const string AssistantBubble = "agent-assistant-bubble";
        public const string UserBubble = "agent-user-bubble";
        public const string StreamingIndicator = "agent-streaming-indicator";
    }
}
