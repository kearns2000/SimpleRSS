namespace SimpleRSS;

/// <summary>
/// Thrown when feed XML cannot be parsed.
/// </summary>
public sealed class FeedParseException : Exception
{
    /// <summary>Feed URL associated with the failure, when known.</summary>
    public string? SourceUrl { get; }

    /// <summary>Creates a new <see cref="FeedParseException"/>.</summary>
    public FeedParseException(string message, string? sourceUrl = null) : base(message)
    {
        SourceUrl = sourceUrl;
    }

    /// <summary>Creates a new <see cref="FeedParseException"/> with an inner exception.</summary>
    public FeedParseException(string message, Exception innerException, string? sourceUrl = null) : base(message, innerException)
    {
        SourceUrl = sourceUrl;
    }
}
