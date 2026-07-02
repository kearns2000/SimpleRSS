using System.Reflection;

namespace SimpleRSS;

internal static class FeedDefaults
{
    internal static string UserAgent
    {
        get
        {
            var version = Assembly.GetExecutingAssembly().GetName().Version;
            return version is null
                ? "SimpleRSS/2.1.0"
                : $"SimpleRSS/{version.Major}.{version.Minor}.{version.Build}";
        }
    }
}
