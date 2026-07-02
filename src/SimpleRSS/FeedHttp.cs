using System.Net.Http.Headers;

namespace SimpleRSS;

internal static class FeedHttp
{
    internal static HttpRequestMessage CreateRequest(string url, FeedReaderOptions options)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.UserAgent.ParseAdd(options.UserAgent);
        return request;
    }

    internal static void ConfigureFeedAcceptHeaders(HttpRequestMessage request)
    {
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/rss+xml"));
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/atom+xml"));
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/xml"));
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/xml"));
    }

    internal static async Task<HttpResponseMessage> SendAsync(
        HttpClient httpClient,
        HttpRequestMessage request,
        TimeSpan requestTimeout,
        CancellationToken cancellationToken)
    {
        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutCts.CancelAfter(requestTimeout);

        try
        {
            return await httpClient.SendAsync(
                request,
                HttpCompletionOption.ResponseHeadersRead,
                timeoutCts.Token).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (OperationCanceledException ex)
        {
            var sourceUrl = request.RequestUri?.ToString() ?? string.Empty;
            throw new FeedHttpException("The feed request timed out.", sourceUrl, innerException: ex);
        }
    }

    internal static FeedCacheMetadata CreateCacheMetadata(HttpResponseMessage response)
    {
        DateTimeOffset? lastModified = null;
        if (response.Content.Headers.LastModified.HasValue)
        {
            lastModified = response.Content.Headers.LastModified.Value;
        }
        else if (response.Headers.TryGetValues("Last-Modified", out var values) &&
                 DateTimeOffset.TryParse(values.FirstOrDefault(), out var parsed))
        {
            lastModified = parsed;
        }

        response.Headers.TryGetValues("ETag", out var etagValues);
        return new FeedCacheMetadata
        {
            ETag = etagValues?.FirstOrDefault(),
            LastModified = lastModified
        };
    }

    internal static void ApplyConditionalHeaders(HttpRequestMessage request, CachedFeed? cachedFeed)
    {
        if (cachedFeed is null)
        {
            return;
        }

        if (!string.IsNullOrWhiteSpace(cachedFeed.ETag))
        {
            request.Headers.IfNoneMatch.ParseAdd(cachedFeed.ETag);
        }

        if (cachedFeed.LastModified.HasValue)
        {
            request.Headers.IfModifiedSince = cachedFeed.LastModified.Value;
        }
    }
}
