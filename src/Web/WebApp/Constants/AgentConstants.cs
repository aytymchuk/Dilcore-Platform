namespace Dilcore.WebApp.Constants;

public static class AgentConstants
{
    public const string PageTitle = "AI Agent";
    public const string HeroPrefix = "How can I assist your";
    public const string HeroHighlight = "Project";
    public const string HeroSuffix = "today?";
    public const string HeroSubtitle = "Select a quick action or start typing below to initialize a new agent workflow.";
    public const string InputPlaceholder = "Message AI Agent...";
    public const string SelectToolsLabel = "Select Tools (MCP)";
    public const string FooterEncryption = "End-to-end encrypted";
    public const string FooterPowered = "Powered by Enterprise LLM";

    public static class QuickActions
    {
        public const string BuildDataSchemaTitle = "Build Data Schema";
        public const string BuildDataSchemaDescription = "Map out complex database architectures with automated relationship detection.";
        public const string BuildDataSchemaPrompt = "Help me build a data schema for my project";

        public const string AutomateLogicTitle = "Automate Logic";
        public const string AutomateLogicDescription = "Generate Python or Node.js scripts to handle repetitive backend tasks.";
        public const string AutomateLogicPrompt = "Help me automate a business logic workflow";

        public const string PerformanceAuditTitle = "Performance Audit";
        public const string PerformanceAuditDescription = "Run deep diagnostic reports on current infrastructure performance.";
        public const string PerformanceAuditPrompt = "Run a performance audit on my workspace";

        public const string SystemGuideTitle = "System Guide";
        public const string SystemGuideDescription = "Generate documentation and API references for your system.";
        public const string SystemGuidePrompt = "Generate a system guide for my workspace";
    }

    public static class TestIds
    {
        public const string HeroSection = "agent-hero-section";
        public const string QuickActionsGrid = "agent-quick-actions-grid";
        public const string QuickActionCard = "agent-quick-action-card";
        public const string MessageInput = "agent-message-input";
        public const string SendButton = "agent-send-button";
        public const string SelectToolsButton = "agent-select-tools-button";
    }
}
