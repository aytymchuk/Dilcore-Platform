namespace Dilcore.WebApi.Infrastructure.Common;

public static class NamingUtils
{
    public static string ToPascalCase(string name)
    {
        if (string.IsNullOrEmpty(name)) return name;
        if (name.Length == 1) return name.ToUpperInvariant();
        return char.ToUpperInvariant(name[0]) + name[1..];
    }

    public static string ToCamelCase(string name)
    {
        if (string.IsNullOrEmpty(name)) return name;
        if (name.Length == 1) return name.ToLowerInvariant();
        return char.ToLowerInvariant(name[0]) + name[1..];
    }
}
