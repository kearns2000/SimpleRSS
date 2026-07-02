namespace SimpleRSS;

/// <summary>
/// A cached feed and its HTTP validation headers.
/// </summary>
public sealed record CachedFeed
{
    /// <summary>The cached feed content.</summary>
    public Feed Feed { get; init; } = new();

    /// <summary>ETag used for conditional requests.</summary>
    public string? ETag { get; init; }

    /// <summary>Last-Modified value used for conditional requests.</summary>
    public DateTimeOffset? LastModified { get; init; }
}
