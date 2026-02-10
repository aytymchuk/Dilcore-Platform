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
    }
}