using System.Collections.Concurrent;

namespace SimpleRSS;

/// <summary>
/// Thread-safe in-memory implementation of <see cref="IFeedResponseCache"/>.
/// </summary>
public sealed class InMemoryFeedResponseCache : IFeedResponseCache
{
    private readonly ConcurrentDictionary<string, CachedFeed> _cache = new(StringComparer.OrdinalIgnoreCase);
    private readonly int _maxEntries;

    /// <summary>
    /// Creates a cache with a maximum number of stored feeds.
    /// </summary>
    public InMemoryFeedResponseCache(int maxEntries = 256)
    {
        if (maxEntries <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maxEntries), "Max entries must be greater than zero.");
        }

        _maxEntries = maxEntries;
    }

    /// <inheritdoc />
    public Task<CachedFeed?> TryGetAsync(string feedUrl, CancellationToken cancellationToken = default)
    {
        Guard.NotNullOrWhiteSpace(feedUrl, nameof(feedUrl));
        _cache.TryGetValue(feedUrl, out var cachedFeed);
        return Task.FromResult<CachedFeed?>(cachedFeed);
    }

    /// <inheritdoc />
    public Task StoreAsync(string feedUrl, CachedFeed cachedFeed, CancellationToken cancellationToken = default)
    {
        Guard.NotNullOrWhiteSpace(feedUrl, nameof(feedUrl));
        Guard.NotNull(cachedFeed, nameof(cachedFeed));

        if (_cache.Count >= _maxEntries && !_cache.ContainsKey(feedUrl))
        {
            var keyToRemove = _cache.Keys.FirstOrDefault();
            if (keyToRemove is not null)
            {
                _cache.TryRemove(keyToRemove, out _);
            }
        }

        _cache[feedUrl] = cachedFeed;
        return Task.CompletedTask;
    }

    /// <summary>Removes all cached entries.</summary>
    public void Clear() => _cache.Clear();
}
