using System.Globalization;

namespace SimpleRSS;

internal static class FeedDateParser
{
    private static readonly string[] Rfc822Formats =
    [
        "ddd, dd MMM yyyy HH:mm:ss zzz",
        "ddd, dd MMM yyyy HH:mm:ss 'GMT'",
        "ddd, dd MMM yyyy HH:mm:ss 'UT'",
        "ddd, dd MMM yyyy HH:mm:ss K",
        "ddd, dd MMM yy HH:mm:ss zzz",
        "ddd, dd MMM yy HH:mm:ss 'GMT'",
        "yyyy-MM-dd'T'HH:mm:ssK",
        "yyyy-MM-dd'T'HH:mm:ss.fffK"
    ];

    internal static DateTimeOffset? TryParse(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var trimmed = value.Trim();

        if (DateTimeOffset.TryParse(trimmed, CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces, out var parsed))
        {
            return parsed;
        }

        if (DateTimeOffset.TryParseExact(
                trimmed,
                Rfc822Formats,
                CultureInfo.InvariantCulture,
                DateTimeStyles.AllowWhiteSpaces,
                out var rfcDate))
        {
            return rfcDate;
        }

        return null;
    }
}
