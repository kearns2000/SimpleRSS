namespace SimpleRSS;

/// <summary>
/// A media enclosure attached to a feed item.
/// </summary>
public sealed record FeedEnclosure
{
    /// <summary>URL of the enclosed media.</summary>
    public string Url { get; init; } = string.Empty;

    /// <summary>MIME type when provided by the feed.</summary>
    public string? Type { get; init; }

    /// <summary>Byte length when provided by the feed.</summary>
    public long? Length { get; init; }
}
