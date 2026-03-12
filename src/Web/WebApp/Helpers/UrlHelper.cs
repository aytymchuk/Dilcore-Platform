namespace Dilcore.WebApp.Helpers;

public static class UrlHelper
{
    public static bool IsLocalUrl(string? url)
    {
        if (string.IsNullOrEmpty(url))
        {
            return false;
        }

        if (url[0] == '/')
        {
            return IsRootedUrl(url);
        }

        if (url[0] == '~' && url.Length > 1 && url[1] == '/')
        {
            return IsAppRelativeUrl(url);
        }

        return false;
    }

    private static bool IsRootedUrl(string url)
    {
        // url is exactly "/"
        if (url.Length == 1)
        {
            return true;
        }

        // url is "/foo" (not "//" and not "/\")
        return url[1] != '/' && url[1] != '\\';
    }

    private static bool IsAppRelativeUrl(string url)
    {
        // url is exactly "~/"
        if (url.Length == 2)
        {
            return true;
        }

        // url is "~/foo" (not "~//" and not "~/\")
        return url[2] != '/' && url[2] != '\\';
    }
}
