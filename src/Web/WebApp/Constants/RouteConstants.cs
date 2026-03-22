namespace Dilcore.WebApp.Constants;

public static class RouteConstants
{
    public const string Home = "/";

    public static class Identity
    {
        public const string Login = "/Account/Login";
        public const string Logout = "/Account/Logout";
    }

    public static class Users
    {
        public const string Register = "/register";
    }

    public static class Workspace
    {
        public const string Prefix = "workspaces";
        public const string TenantRoot = "/workspaces/{tenant}";
        public const string RouteParameter = "tenant";

        public static string ForTenant(string systemName) => $"/workspaces/{systemName}";

        public static class Admin
        {
            public const string Prefix = "admin";
            public const string TenantAdminRoot = "/workspaces/{tenant}/admin";

            public static string ForTenant(string systemName) => $"/workspaces/{systemName}/admin";

            public static class Agent
            {
                public const string Prefix = "agent";
                public const string Route = "/workspaces/{tenant}/admin/agent";

                public static string ForTenant(string systemName) => $"/workspaces/{systemName}/admin/agent";
            }
        }
    }
}