using System.Net;
using System.Text.RegularExpressions;

namespace SimpleRSS;

internal static class FeedHtmlUtility
{
    private static readonly Regex HtmlTagRegex = new("<[^>]+>", RegexOptions.Compiled | RegexOptions.CultureInvariant);

    internal static string? Normalize(string? value, bool decodeEntities, bool stripHtml)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return value;
        }

        var normalized = value;
        if (decodeEntities)
        {
            normalized = WebUtility.HtmlDecode(normalized);
        }

        if (stripHtml)
        {
            normalized = HtmlTagRegex.Replace(normalized, string.Empty);
            normalized = Regex.Replace(normalized, "\\s+", " ").Trim();
        }

        return normalized;
    }
}
