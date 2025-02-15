namespace Ecommerce_site.Util;

public class ValidPath
{
    public static bool BeValidPathOrUri(string input)
    {
        return IsValidServerPath(input) || IsValidCloudUri(input);
    }

    private static bool IsValidServerPath(string path)
    {
        // Check for Linux-style paths
        var isLinuxPath = path.StartsWith("/")
                          && !path.Contains("://")
                          && path.IndexOfAny(Path.GetInvalidPathChars()) == -1;

        // Check for Windows-style paths
        var isWindowsPath = (path.Length >= 3 && char.IsLetter(path[0]) && path[1] == ':' && path[2] == '\\')
                            || path.StartsWith(@"\\")
                            && !path.Contains("://")
                            && path.IndexOfAny(Path.GetInvalidPathChars()) == -1;

        return isLinuxPath || isWindowsPath;
    }

    private static bool IsValidCloudUri(string uri)
    {
        return Uri.TryCreate(uri, UriKind.Absolute, out Uri? result)
               && (result.Scheme == Uri.UriSchemeHttp || result.Scheme == Uri.UriSchemeHttps);
    }
}