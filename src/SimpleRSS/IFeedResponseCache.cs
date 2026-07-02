namespace SimpleRSS;

/// <summary>
/// Stores feed responses for conditional HTTP requests.
/// </summary>
public interface IFeedResponseCache
{
    /// <summary>Gets a cached feed for the given URL, if present.</summary>
    Task<CachedFeed?> TryGetAsync(string feedUrl, CancellationToken cancellationToken = default);

    /// <summary>Stores a feed and its HTTP cache headers.</summary>
    Task StoreAsync(string feedUrl, CachedFeed cachedFeed, CancellationToken cancellationToken = default);
}
