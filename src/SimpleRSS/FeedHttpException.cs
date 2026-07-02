using System.Net;

namespace SimpleRSS;

/// <summary>
/// Thrown when a feed cannot be retrieved over HTTP.
/// </summary>
public sealed class FeedHttpException : Exception
{
    /// <summary>Feed URL associated with the failure.</summary>
    public string SourceUrl { get; }

    /// <summary>HTTP status code when available.</summary>
    public HttpStatusCode? StatusCode { get; }

    /// <summary>Creates a new <see cref="FeedHttpException"/>.</summary>
    public FeedHttpException(string message, string sourceUrl, HttpStatusCode? statusCode = null, Exception? innerException = null)
        : base(message, innerException)
    {
        SourceUrl = sourceUrl;
        StatusCode = statusCode;
    }
}
