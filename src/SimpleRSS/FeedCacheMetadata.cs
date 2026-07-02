namespace SimpleRSS;

/// <summary>
/// HTTP caching metadata for a retrieved feed.
/// </summary>
public sealed record FeedCacheMetadata
{
    /// <summary>ETag header value from the response.</summary>
    public string? ETag { get; init; }

    /// <summary>Last-Modified header value from the response.</summary>
    public DateTimeOffset? LastModified { get; init; }
}
