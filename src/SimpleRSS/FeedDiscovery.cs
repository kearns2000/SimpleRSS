using System.Text;
using System.Text.RegularExpressions;

namespace SimpleRSS;

/// <summary>
/// Discovers syndication feed URLs from website HTML.
/// </summary>
public sealed class FeedDiscovery
{
    private static readonly Regex FeedLinkRegex = new(
        """<link\b[^>]*>""",
        RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private readonly HttpClient _httpClient;
    private readonly FeedReaderOptions _options;

    /// <summary>
    /// Creates a discovery helper with a shared <see cref="HttpClient"/>.
    /// </summary>
    public FeedDiscovery(HttpClient httpClient, FeedReaderOptions? options = null)
    {
        Guard.NotNull(httpClient, nameof(httpClient));
        _httpClient = httpClient;
        _options = options ?? new FeedReaderOptions();
    }

    /// <summary>
    /// Finds the first RSS or Atom feed URL linked from a website.
    /// </summary>
    public async Task<string?> DiscoverFeedUrlAsync(string siteUrl, CancellationToken cancellationToken = default)
    {
        Guard.NotNullOrWhiteSpace(siteUrl, nameof(siteUrl));

        using var request = FeedHttp.CreateRequest(siteUrl, _options);
        request.Headers.Accept.Clear();
        request.Headers.Accept.ParseAdd("text/html");

        using var response = await FeedHttp.SendAsync(_httpClient, request, _options.RequestTimeout, cancellationToken)
            .ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
        {
            throw new FeedHttpException(
                $"Failed to discover feed URL. HTTP {(int)response.StatusCode} {response.ReasonPhrase}.",
                siteUrl,
                response.StatusCode);
        }

        await using var responseStream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
        await using var limitedStream = await FeedStreamUtility
            .CopyLimitedAsync(responseStream, _options.MaxResponseBytes, cancellationToken)
            .ConfigureAwait(false);

        using var reader = new StreamReader(limitedStream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
        var html = await reader.ReadToEndAsync(cancellationToken).ConfigureAwait(false);
        return DiscoverFromHtml(html, siteUrl).FirstOrDefault();
    }

    /// <summary>
    /// Finds feed URLs from HTML content.
    /// </summary>
    public IReadOnlyList<string> DiscoverFromHtml(string html, string? baseUrl = null)
    {
        Guard.NotNull(html, nameof(html));

        var matches = FeedLinkRegex.Matches(html);
        var feedUrls = new List<string>();

        foreach (Match match in matches)
        {
            var tag = match.Value;
            if (!HasAlternateRelationship(tag) || !IsFeedType(tag))
            {
                continue;
            }

            var href = ExtractAttribute(tag, "href");
            if (string.IsNullOrWhiteSpace(href))
            {
                continue;
            }

            feedUrls.Add(ResolveUrl(href!, baseUrl));
        }

        return feedUrls.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
    }

    private static bool HasAlternateRelationship(string tag)
    {
        var rel = ExtractAttribute(tag, "rel");
        if (string.IsNullOrWhiteSpace(rel))
        {
            return false;
        }

        return rel
            .Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Any(token => token.Equals("alternate", StringComparison.OrdinalIgnoreCase));
    }

    private static bool IsFeedType(string tag)
    {
        var type = ExtractAttribute(tag, "type");
        return type is not null &&
               (type.Contains("rss", StringComparison.OrdinalIgnoreCase) ||
                type.Contains("atom", StringComparison.OrdinalIgnoreCase) ||
                type.Contains("xml", StringComparison.OrdinalIgnoreCase));
    }

    private static string? ExtractAttribute(string tag, string attributeName)
    {
        var match = Regex.Match(
            tag,
            $"{attributeName}\\s*=\\s*(?:\"(?<value>[^\"]*)\"|'(?<value>[^']*)'|(?<value>[^\\s>]+))",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

        return match.Success ? match.Groups["value"].Value : null;
    }

    private static string ResolveUrl(string href, string? baseUrl)
    {
        if (href.Contains("://", StringComparison.Ordinal) &&
            Uri.TryCreate(href, UriKind.Absolute, out var absolute))
        {
            return absolute.ToString();
        }

        if (!string.IsNullOrWhiteSpace(baseUrl) &&
            Uri.TryCreate(baseUrl, UriKind.Absolute, out var baseUri) &&
            Uri.TryCreate(baseUri, href, out var resolved))
        {
            return resolved.ToString();
        }

        return href;
    }
}
