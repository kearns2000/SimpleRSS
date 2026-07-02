namespace SimpleRSS;

/// <summary>
/// A syndication feed with channel metadata and entries.
/// </summary>
public sealed record Feed
{
    /// <summary>Feed title.</summary>
    public string? Title { get; init; }

    /// <summary>Feed link.</summary>
    public string? Link { get; init; }

    /// <summary>Feed description.</summary>
    public string? Description { get; init; }

    /// <summary>Source URL when the feed was retrieved over HTTP.</summary>
    public string? SourceUrl { get; init; }

    /// <summary>Entries contained in the feed.</summary>
    public IReadOnlyList<FeedItem> Items { get; init; } = Array.Empty<FeedItem>();

    /// <summary>HTTP cache metadata from the most recent response.</summary>
    public FeedCacheMetadata? CacheMetadata { get; init; }
}
