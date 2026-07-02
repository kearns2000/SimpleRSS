namespace SimpleRSS;

/// <summary>
/// Combines items from multiple feed results.
/// </summary>
public static class FeedCombiner
{
    /// <summary>
    /// Returns items from successful feeds, optionally interleaved.
    /// </summary>
    public static IReadOnlyList<FeedItem> CombineItems(
        IEnumerable<FeedResult> results,
        bool interleave = false)
    {
        Guard.NotNull(results, nameof(results));

        var feeds = results
            .Where(result => result.IsSuccess && result.Feed is not null)
            .Select(result => result.Feed!.Items)
            .ToList();

        if (feeds.Count == 0)
        {
            return Array.Empty<FeedItem>();
        }

        return interleave ? Interleave(feeds) : feeds.SelectMany(items => items).ToList();
    }

    private static IReadOnlyList<FeedItem> Interleave(IReadOnlyList<IReadOnlyList<FeedItem>> feeds)
    {
        var result = new List<FeedItem>();
        var maxLength = feeds.Max(feed => feed.Count);

        for (var index = 0; index < maxLength; index++)
        {
            foreach (var feed in feeds)
            {
                if (index < feed.Count)
                {
                    result.Add(feed[index]);
                }
            }
        }

        return result;
    }
}
