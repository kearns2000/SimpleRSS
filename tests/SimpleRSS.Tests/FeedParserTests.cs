namespace SimpleRSS.Tests;

using System.Net;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

public sealed class FeedParserTests
{
    private const string RssFeed = """
        <?xml version="1.0" encoding="UTF-8"?>
        <rss version="2.0" xmlns:dc="http://purl.org/dc/elements/1.1/" xmlns:content="http://purl.org/rss/1.0/modules/content/" xmlns:media="http://search.yahoo.com/mrss/">
          <channel>
            <title>Example RSS</title>
            <link>https://example.com</link>
            <description>Example channel</description>
            <item>
              <title>First &amp; post</title>
              <link>https://example.com/1</link>
              <pubDate>Mon, 01 Jan 2024 12:00:00 +0000</pubDate>
              <description><![CDATA[<p>Hello world</p>]]></description>
              <content:encoded><![CDATA[<p>Full content</p>]]></content:encoded>
              <guid isPermaLink="true">https://example.com/1</guid>
              <dc:creator>Jane Doe</dc:creator>
              <category>News</category>
              <enclosure url="https://example.com/audio.mp3" type="audio/mpeg" length="12345" />
              <media:thumbnail url="https://example.com/thumb.jpg" />
            </item>
            <item>
              <title>Second post</title>
              <guid isPermaLink="false">post-2</guid>
              <link>https://example.com/2</link>
              <description>Another update</description>
            </item>
          </channel>
        </rss>
        """;

    private const string AtomFeed = """
        <?xml version="1.0" encoding="utf-8"?>
        <feed xmlns="http://www.w3.org/2005/Atom">
          <title>Example Atom</title>
          <link rel="alternate" href="https://example.com/atom" />
          <subtitle>Atom channel</subtitle>
          <entry>
            <title>Atom entry</title>
            <link rel="alternate" href="https://example.com/atom/1" />
            <link rel="enclosure" href="https://example.com/file.zip" type="application/zip" length="99" />
            <id>urn:example:1</id>
            <published>2024-01-02T10:00:00Z</published>
            <updated>2024-01-03T10:00:00Z</updated>
            <summary>Atom summary</summary>
            <content>Atom content</content>
            <author><name>John Smith</name></author>
            <category term="Tech" />
          </entry>
        </feed>
        """;

    private const string RdfFeed = """
        <?xml version="1.0" encoding="UTF-8"?>
        <rdf:RDF xmlns:rdf="http://www.w3.org/1999/02/22-rdf-syntax-ns#" xmlns="http://purl.org/rss/1.0/">
          <channel rdf:about="https://example.com/rdf">
            <title>RDF Feed</title>
            <link>https://example.com/rdf</link>
            <description>RDF channel</description>
          </channel>
          <item rdf:about="https://example.com/rdf/1">
            <title>RDF item</title>
            <link>https://example.com/rdf/1</link>
            <description>RDF description</description>
            <category rdf:resource="https://example.com/category/tech" />
          </item>
        </rdf:RDF>
        """;

    [Fact]
    public void Parse_RssFeed_ReturnsFeedMetadataAndItems()
    {
        var feed = FeedReader.Parse(RssFeed);

        Assert.Equal("Example RSS", feed.Title);
        Assert.Equal("https://example.com", feed.Link);
        Assert.Equal("Example channel", feed.Description);
        Assert.Equal(2, feed.Items.Count);

        var first = feed.Items[0];
        Assert.Equal("First & post", first.Title);
        Assert.Equal("https://example.com/1", first.Link);
        Assert.Equal("<p>Hello world</p>", first.Summary);
        Assert.Equal("<p>Full content</p>", first.Content);
        Assert.Equal("https://example.com/1", first.Id);
        Assert.Equal("Jane Doe", first.Author);
        Assert.Equal(["News"], first.Categories);
        Assert.Equal(new DateTimeOffset(2024, 1, 1, 12, 0, 0, TimeSpan.Zero), first.Published);
        Assert.Single(first.Enclosures);
        Assert.Equal("https://example.com/audio.mp3", first.Enclosures[0].Url);
        Assert.Equal("https://example.com/thumb.jpg", first.ImageUrl);
    }

    [Fact]
    public void Parse_RssFeed_StripsHtml_WhenConfigured()
    {
        var feed = FeedReader.Parse(RssFeed, new FeedReaderOptions
        {
            StripHtmlFromText = true,
            DecodeHtmlEntities = true
        });

        Assert.Equal("Hello world", feed.Items[0].Summary);
    }

    [Fact]
    public void Parse_RssFeed_UsesLinkWhenGuidIsNotPermalink()
    {
        var feed = FeedReader.Parse(RssFeed);
        Assert.Equal("https://example.com/2", feed.Items[1].Link);
        Assert.Equal("post-2", feed.Items[1].Id);
    }

    [Fact]
    public void Parse_AtomFeed_ReturnsItems()
    {
        var feed = FeedReader.Parse(AtomFeed);

        Assert.Equal("Example Atom", feed.Title);
        Assert.Equal("https://example.com/atom", feed.Link);
        Assert.Single(feed.Items);

        var item = feed.Items[0];
        Assert.Equal("Atom entry", item.Title);
        Assert.Equal("https://example.com/atom/1", item.Link);
        Assert.Equal("Atom summary", item.Summary);
        Assert.Equal("Atom content", item.Content);
        Assert.Equal("urn:example:1", item.Id);
        Assert.Equal("John Smith", item.Author);
        Assert.Equal(["Tech"], item.Categories);
        Assert.Equal(new DateTimeOffset(2024, 1, 2, 10, 0, 0, TimeSpan.Zero), item.Published);
        Assert.Equal(new DateTimeOffset(2024, 1, 3, 10, 0, 0, TimeSpan.Zero), item.Updated);
        Assert.Single(item.Enclosures);
    }

    [Fact]
    public void Parse_RdfFeed_ReturnsItemsAndCategories()
    {
        var feed = FeedReader.Parse(RdfFeed);

        Assert.Equal("RDF Feed", feed.Title);
        Assert.Single(feed.Items);
        Assert.Equal("RDF item", feed.Items[0].Title);
        Assert.Equal("https://example.com/rdf/1", feed.Items[0].Link);
        Assert.Equal("https://example.com/rdf/1", feed.Items[0].Id);
        Assert.Equal(["https://example.com/category/tech"], feed.Items[0].Categories);
    }

    [Fact]
    public void Parse_InvalidXml_ThrowsFeedParseException()
    {
        var exception = Assert.Throws<FeedParseException>(() => FeedReader.Parse("<rss><unclosed>"));
        Assert.Contains("valid XML", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ParseAsync_Stream_ParsesNonUtf8Encoding()
    {
        var xmlBytes = Encoding.Latin1.GetBytes("""
            <?xml version="1.0" encoding="iso-8859-1"?>
            <rss version="2.0">
              <channel>
                <item><title>Café</title></item>
              </channel>
            </rss>
            """);

        await using var stream = new MemoryStream(xmlBytes);
        var feed = await FeedReader.ParseAsync(stream);

        Assert.Equal("Café", feed.Items[0].Title);
    }

    [Fact]
    public async Task ParseAsync_ThrowsWhenCancelled()
    {
        await using var stream = new MemoryStream(Encoding.UTF8.GetBytes(RssFeed));
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        await Assert.ThrowsAsync<OperationCanceledException>(() =>
            FeedReader.ParseAsync(stream, cancellationToken: cts.Token));
    }

    [Fact]
    public void DiscoverFromHtml_FindsFeedLinks()
    {
        var discovery = new FeedDiscovery(new HttpClient(new HttpClientHandler()));
        var urls = discovery.DiscoverFromHtml(
            """
            <html>
              <head>
                <link rel="alternate" type="application/rss+xml" href="/feed.xml" />
                <link rel="alternate application/rss+xml" type="application/rss+xml" href="/feed2.xml" />
                <link rel="alternate" type="application/atom+xml" href="https://cdn.example.com/atom.xml" />
              </head>
            </html>
            """,
            "https://example.com/blog");

        Assert.Equal(3, urls.Count);
        Assert.Equal("https://example.com/feed.xml", urls[0]);
        Assert.Equal("https://example.com/feed2.xml", urls[1]);
        Assert.Equal("https://cdn.example.com/atom.xml", urls[2]);
    }

    [Fact]
    public async Task GetCombinedItemsAsync_InterleavesItems_WhenEnabled()
    {
        using var handler = new QueueingHttpMessageHandler();
        handler.Enqueue(RssFeed);
        handler.Enqueue("""
            <?xml version="1.0" encoding="UTF-8"?>
            <rss version="2.0">
              <channel>
                <item><title>A1</title></item>
                <item><title>A2</title></item>
              </channel>
            </rss>
            """);

        using var reader = new FeedReader(
            new HttpClient(handler),
            new FeedReaderOptions { InterleaveMultipleFeeds = true });

        var items = await reader.GetCombinedItemsAsync(["https://example.com/feed-b", "https://example.com/feed-a"]);

        Assert.Equal(4, items.Count);
        Assert.Equal("First & post", items[0].Title);
        Assert.Equal("A1", items[1].Title);
        Assert.Equal("Second post", items[2].Title);
        Assert.Equal("A2", items[3].Title);
    }

    [Fact]
    public async Task GetFeedsAsync_ContinuesOnError_WhenConfigured()
    {
        using var handler = new QueueingHttpMessageHandler();
        handler.Enqueue(RssFeed);
        handler.EnqueueStatus(HttpStatusCode.NotFound);

        using var reader = new FeedReader(
            new HttpClient(handler),
            new FeedReaderOptions { ContinueOnError = true });

        var results = await reader.GetFeedsAsync(["https://example.com/good", "https://example.com/bad"]);

        Assert.Equal(2, results.Count);
        Assert.True(results[0].IsSuccess);
        Assert.False(results[1].IsSuccess);
        Assert.IsType<FeedHttpException>(results[1].Error);
    }

    [Fact]
    public async Task GetFeedsAsync_ThrowsWhenContinueOnErrorIsFalse()
    {
        using var handler = new QueueingHttpMessageHandler();
        handler.EnqueueStatus(HttpStatusCode.NotFound);

        using var reader = new FeedReader(
            new HttpClient(handler),
            new FeedReaderOptions { ContinueOnError = false });

        await Assert.ThrowsAsync<FeedHttpException>(() =>
            reader.GetFeedsAsync(["https://example.com/bad"]));
    }

    [Fact]
    public async Task GetFeedAsync_UsesCachedFeed_OnNotModified()
    {
        var cache = new InMemoryFeedResponseCache();
        var cachedFeed = FeedReader.Parse(RssFeed) with { SourceUrl = "https://example.com/feed" };
        await cache.StoreAsync(
            "https://example.com/feed",
            new CachedFeed { Feed = cachedFeed, ETag = "\"v1\"" });

        using var handler = new NotModifiedHttpMessageHandler();
        using var reader = new FeedReader(
            new HttpClient(handler),
            new FeedReaderOptions { ResponseCache = cache });

        var feed = await reader.GetFeedAsync("https://example.com/feed");

        Assert.Equal("Example RSS", feed.Title);
        Assert.Equal(2, feed.Items.Count);
    }

    [Fact]
    public async Task GetFeedAsync_ThrowsWhenResponseExceedsMaxBytes()
    {
        using var handler = new LargeResponseHttpMessageHandler(200);
        using var reader = new FeedReader(
            new HttpClient(handler),
            new FeedReaderOptions { MaxResponseBytes = 100 });

        var exception = await Assert.ThrowsAsync<FeedParseException>(() =>
            reader.GetFeedAsync("https://example.com/feed"));

        Assert.Contains("maximum allowed size", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task GetFeedAsync_PropagatesCancellation()
    {
        using var handler = new DelayingHttpMessageHandler();
        using var reader = new FeedReader(new HttpClient(handler));
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            reader.GetFeedAsync("https://example.com/feed", cts.Token));
    }

    [Fact]
    public async Task GetFeedAsync_EnforcesRequestTimeout_WithSharedHttpClient()
    {
        using var handler = new DelayingHttpMessageHandler();
        using var reader = new FeedReader(
            new HttpClient(handler),
            new FeedReaderOptions { RequestTimeout = TimeSpan.FromMilliseconds(50) });

        await Assert.ThrowsAsync<FeedHttpException>(() =>
            reader.GetFeedAsync("https://example.com/feed"));
    }

    [Fact]
    public void AddSimpleRss_RegistersFeedReader()
    {
        var services = new ServiceCollection();
        services.AddSimpleRss();

        using var provider = services.BuildServiceProvider();
        var reader = provider.GetRequiredService<FeedReader>();
        var discovery = provider.GetRequiredService<FeedDiscovery>();

        Assert.NotNull(reader);
        Assert.NotNull(discovery);
        Assert.Null(provider.GetService<IFeedResponseCache>());
    }

    [Fact]
    public void AddSimpleRss_RegistersCacheOnlyWhenEnabled()
    {
        var services = new ServiceCollection();
        services.AddSimpleRss(options => options with { EnableResponseCache = true });

        using var provider = services.BuildServiceProvider();

        Assert.NotNull(provider.GetService<IFeedResponseCache>());
    }

    private sealed class QueueingHttpMessageHandler : HttpMessageHandler
    {
        private readonly Queue<object> _responses = new();

        public void Enqueue(string content) => _responses.Enqueue(content);

        public void EnqueueStatus(HttpStatusCode statusCode) => _responses.Enqueue(statusCode);

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (_responses.Count == 0)
            {
                throw new InvalidOperationException("No queued HTTP responses remain.");
            }

            var next = _responses.Dequeue();
            if (next is HttpStatusCode statusCode)
            {
                return Task.FromResult(new HttpResponseMessage(statusCode));
            }

            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent((string)next)
            });
        }
    }

    private sealed class NotModifiedHttpMessageHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotModified));
    }

    private sealed class LargeResponseHttpMessageHandler(int byteCount) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var content = new string('a', byteCount);
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(content)
            });
        }
    }

    private sealed class DelayingHttpMessageHandler : HttpMessageHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken).ConfigureAwait(false);
            return new HttpResponseMessage(HttpStatusCode.OK);
        }
    }
}
