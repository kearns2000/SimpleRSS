namespace SimpleRSS;

/// <summary>
/// A single entry from an RSS or Atom syndication feed.
/// </summary>
public sealed record FeedItem
{
    /// <summary>Entry title.</summary>
    public string? Title { get; init; }

    /// <summary>Link to the full article or resource.</summary>
    public string? Link { get; init; }

    /// <summary>Short summary when provided separately from full content.</summary>
    public string? Summary { get; init; }

    /// <summary>Full content when provided separately from summary.</summary>
    public string? Content { get; init; }

    /// <summary>Summary or content, whichever is available.</summary>
    public string? Description => Summary ?? Content;

    /// <summary>Parsed publication date when available.</summary>
    public DateTimeOffset? Published { get; init; }

    /// <summary>Raw publication date text from the feed.</summary>
    public string? PublishedDateRaw { get; init; }

    /// <summary>Parsed updated date when available.</summary>
    public DateTimeOffset? Updated { get; init; }

    /// <summary>Raw updated date text from the feed.</summary>
    public string? UpdatedDateRaw { get; init; }

    /// <summary>Unique identifier when provided by the feed.</summary>
    public string? Id { get; init; }

    /// <summary>Author name when provided by the feed.</summary>
    public string? Author { get; init; }

    /// <summary>Categories or tags associated with the entry.</summary>
    public IReadOnlyList<string> Categories { get; init; } = Array.Empty<string>();

    /// <summary>Media enclosures attached to the entry.</summary>
    public IReadOnlyList<FeedEnclosure> Enclosures { get; init; } = Array.Empty<FeedEnclosure>();

    /// <summary>Primary image URL when available.</summary>
    public string? ImageUrl { get; init; }
}
