namespace Dilcore.WebApp.Helpers;

public static class UrlHelper
{
    public static bool IsLocalUrl(string? url)
    {
        if (string.IsNullOrEmpty(url))
        {
            return false;
        }

        // Allows "/" or "/foo" but not "//" or "/\"
        if (url[0] == '/')
        {
            // url is exactly "/"
            if (url.Length == 1)
            {
                return true;
            }

            // url is "/foo" (not "//" and not "/\")
            if (url[1] != '/' && url[1] != '\\')
            {
                return true;
            }

            return false;
        }

        // Allows "~/" or "~/foo"
        if (url[0] == '~' && url.Length > 1 && url[1] == '/')
        {
            // url is exactly "~/"
            if (url.Length == 2)
            {
                return true;
            }

            // url is "~/foo" (not "~//" and not "~/\")
            if (url[2] != '/' && url[2] != '\\')
            {
                return true;
            }

            return false;
        }

        return false;
    }
}
