using System.Net;

namespace SimpleRSS;

/// <summary>
/// Reads RSS and Atom syndication feeds from URLs, streams, or raw XML.
/// </summary>
public sealed class FeedReader : IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly bool _ownsHttpClient;
    private readonly FeedReaderOptions _options;
    private FeedDiscovery? _discovery;

    /// <summary>
    /// Creates a reader with a shared <see cref="HttpClient"/> instance.
    /// </summary>
    public FeedReader(HttpClient httpClient, FeedReaderOptions? options = null)
    {
        Guard.NotNull(httpClient, nameof(httpClient));
        _httpClient = httpClient;
        _ownsHttpClient = false;
        _options = options ?? new FeedReaderOptions();
    }

    /// <summary>
    /// Creates a reader with a new <see cref="HttpClient"/> instance.
    /// </summary>
    public FeedReader(FeedReaderOptions? options = null)
    {
        _options = options ?? new FeedReaderOptions();
        _httpClient = new HttpClient
        {
            Timeout = Timeout.InfiniteTimeSpan
        };
        _ownsHttpClient = true;
    }

    /// <summary>
    /// Retrieves and parses a feed from a URL.
    /// </summary>
    public Task<Feed> GetFeedAsync(string feedUrl, CancellationToken cancellationToken = default) =>
        GetFeedInternalAsync(feedUrl, cancellationToken);

    /// <summary>
    /// Retrieves and parses multiple feeds in parallel.
    /// </summary>
    public async Task<IReadOnlyList<FeedResult>> GetFeedsAsync(
        IEnumerable<string> feedUrls,
        CancellationToken cancellationToken = default)
    {
        Guard.NotNull(feedUrls, nameof(feedUrls));

        var urls = feedUrls.Where(url => !string.IsNullOrWhiteSpace(url)).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        if (urls.Count == 0)
        {
            return Array.Empty<FeedResult>();
        }

        using var semaphore = new SemaphoreSlim(Math.Max(1, _options.MaxConcurrentRequests));
        var tasks = urls.Select(async url =>
        {
            await semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                var feed = await GetFeedInternalAsync(url, cancellationToken).ConfigureAwait(false);
                return new FeedResult { SourceUrl = url, Feed = feed };
            }
            catch (Exception ex) when (_options.ContinueOnError)
            {
                return new FeedResult { SourceUrl = url, Error = ex };
            }
            finally
            {
                semaphore.Release();
            }
        });

        return await Task.WhenAll(tasks).ConfigureAwait(false);
    }

    /// <summary>
    /// Retrieves multiple feeds and returns a flattened list of items.
    /// </summary>
    public async Task<IReadOnlyList<FeedItem>> GetCombinedItemsAsync(
        IEnumerable<string> feedUrls,
        CancellationToken cancellationToken = default)
    {
        var results = await GetFeedsAsync(feedUrls, cancellationToken).ConfigureAwait(false);
        return FeedCombiner.CombineItems(results, _options.InterleaveMultipleFeeds);
    }

    /// <summary>
    /// Discovers and retrieves the first feed linked from a website.
    /// </summary>
    public async Task<Feed> DiscoverAndGetFeedAsync(string siteUrl, CancellationToken cancellationToken = default)
    {
        var feedUrl = await Discovery.DiscoverFeedUrlAsync(siteUrl, cancellationToken).ConfigureAwait(false);
        if (string.IsNullOrWhiteSpace(feedUrl))
        {
            throw new FeedHttpException("No RSS or Atom feed link was found on the page.", siteUrl);
        }

        return await GetFeedAsync(feedUrl, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Parses feed XML from a stream.
    /// </summary>
    public static Task<Feed> ParseAsync(Stream stream, FeedReaderOptions? options = null, CancellationToken cancellationToken = default)
    {
        Guard.NotNull(stream, nameof(stream));
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(FeedParser.Parse(stream, options));
    }

    /// <summary>
    /// Parses feed XML from a string.
    /// </summary>
    public static Feed Parse(string xml, FeedReaderOptions? options = null) =>
        FeedParser.Parse(xml, options);

    private FeedDiscovery Discovery => _discovery ??= new FeedDiscovery(_httpClient, _options);

    private async Task<Feed> GetFeedInternalAsync(string feedUrl, CancellationToken cancellationToken)
    {
        Guard.NotNullOrWhiteSpace(feedUrl, nameof(feedUrl));

        CachedFeed? cachedFeed = null;
        if (_options.ResponseCache is not null)
        {
            cachedFeed = await _options.ResponseCache.TryGetAsync(feedUrl, cancellationToken).ConfigureAwait(false);
        }

        using var request = FeedHttp.CreateRequest(feedUrl, _options);
        FeedHttp.ConfigureFeedAcceptHeaders(request);
        FeedHttp.ApplyConditionalHeaders(request, cachedFeed);

        using var response = await FeedHttp.SendAsync(_httpClient, request, _options.RequestTimeout, cancellationToken)
            .ConfigureAwait(false);

        if (response.StatusCode == HttpStatusCode.NotModified && cachedFeed is not null)
        {
            return cachedFeed.Feed with { SourceUrl = feedUrl };
        }

        if (!response.IsSuccessStatusCode)
        {
            throw new FeedHttpException(
                $"Failed to retrieve feed. HTTP {(int)response.StatusCode} {response.ReasonPhrase}.",
                feedUrl,
                response.StatusCode);
        }

        await using var responseStream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
        await using var limitedStream = await FeedStreamUtility
            .CopyLimitedAsync(responseStream, _options.MaxResponseBytes, cancellationToken)
            .ConfigureAwait(false);

        var feed = FeedParser.Parse(limitedStream, _options, feedUrl);
        var cacheMetadata = FeedHttp.CreateCacheMetadata(response);
        feed = feed with
        {
            SourceUrl = feedUrl,
            CacheMetadata = cacheMetadata
        };

        if (_options.ResponseCache is not null)
        {
            await _options.ResponseCache.StoreAsync(
                feedUrl,
                new CachedFeed
                {
                    Feed = feed,
                    ETag = cacheMetadata.ETag,
                    LastModified = cacheMetadata.LastModified
                },
                cancellationToken).ConfigureAwait(false);
        }

        return feed;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_ownsHttpClient)
        {
            _httpClient.Dispose();
        }
    }
}
