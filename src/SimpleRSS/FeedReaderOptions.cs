namespace SimpleRSS;

/// <summary>
/// Options for retrieving and parsing feeds.
/// </summary>
public sealed record FeedReaderOptions
{
    /// <summary>
    /// When <see langword="true"/>, combined items from multiple feeds are interleaved.
    /// </summary>
    public bool InterleaveMultipleFeeds { get; init; }

    /// <summary>HTTP request timeout. Defaults to 30 seconds.</summary>
    public TimeSpan RequestTimeout { get; init; } = TimeSpan.FromSeconds(30);

    /// <summary>User-Agent header sent with HTTP requests.</summary>
    public string UserAgent { get; init; } = FeedDefaults.UserAgent;

    /// <summary>Maximum number of feeds fetched in parallel. Defaults to 4.</summary>
    public int MaxConcurrentRequests { get; init; } = 4;

    /// <summary>
    /// When <see langword="true"/>, <see cref="FeedReader.GetFeedsAsync"/> returns successful and failed feeds instead of throwing.
    /// </summary>
    public bool ContinueOnError { get; init; } = true;

    /// <summary>Decodes HTML entities in text fields. Defaults to <see langword="true"/>.</summary>
    public bool DecodeHtmlEntities { get; init; } = true;

    /// <summary>Strips HTML tags from text fields. Defaults to <see langword="false"/>.</summary>
    public bool StripHtmlFromText { get; init; }

    /// <summary>Maximum number of characters allowed in feed XML. Defaults to 10 million.</summary>
    public long MaxXmlCharacterCount { get; init; } = 10_000_000;

    /// <summary>Maximum number of bytes read from HTTP responses. Defaults to 40 MB.</summary>
    public long MaxResponseBytes { get; init; } = 40_000_000;

    /// <summary>
    /// When <see langword="true"/>, registers and uses an in-memory response cache in dependency injection.
    /// </summary>
    public bool EnableResponseCache { get; init; }

    /// <summary>Optional cache used for conditional HTTP requests.</summary>
    public IFeedResponseCache? ResponseCache { get; init; }
}
