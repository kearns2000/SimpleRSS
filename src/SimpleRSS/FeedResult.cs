namespace SimpleRSS;

/// <summary>
/// The result of retrieving a single feed from a multi-feed request.
/// </summary>
public sealed record FeedResult
{
    /// <summary>Feed URL that was requested.</summary>
    public string SourceUrl { get; init; } = string.Empty;

    /// <summary>Parsed feed when retrieval succeeded.</summary>
    public Feed? Feed { get; init; }

    /// <summary>Error when retrieval or parsing failed.</summary>
    public Exception? Error { get; init; }

    /// <summary>Whether the feed was retrieved and parsed successfully.</summary>
    public bool IsSuccess => Error is null && Feed is not null;
}
