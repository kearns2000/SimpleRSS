# SimpleRSS

[![NuGet](https://img.shields.io/nuget/v/SimpleRSS.svg)](https://www.nuget.org/packages/SimpleRSS)
[![NuGet Downloads](https://img.shields.io/nuget/dt/SimpleRSS.svg)](https://www.nuget.org/packages/SimpleRSS)
[![CI](https://github.com/kearns2000/SimpleRSS/actions/workflows/ci.yml/badge.svg)](https://github.com/kearns2000/SimpleRSS/actions/workflows/ci.yml)
[![License](https://img.shields.io/github/license/kearns2000/SimpleRSS.svg)](LICENSE)
[![.NET](https://img.shields.io/badge/.NET-8%2B-512BD4)](https://dotnet.microsoft.com/download)

A lightweight .NET RSS feed reader, parser, and aggregator for RSS 2.0, Atom 1.0, and RDF syndication feeds.

Version 2.x is a modern rewrite for .NET 8+ with async HTTP support, feed metadata, typed dates, caching, feed discovery, parallel multi-feed fetching, and dependency injection.

**Keywords:** RSS reader, RSS parser, Atom reader, Atom parser, syndication feed, feed aggregator, XML feed parser, HTTP feed fetcher, podcast RSS, feed discovery, .NET 8, dependency injection.

## Install

```bash
dotnet add package SimpleRSS
```

## Quick start

```csharp
using SimpleRSS;

using var reader = new FeedReader();

var feed = await reader.GetFeedAsync("https://example.com/feed.rss");
Console.WriteLine(feed.Title);

foreach (var item in feed.Items)
{
    Console.WriteLine($"{item.Published:g}\t{item.Title}");
    Console.WriteLine(item.Link);
}
```

> **Note:** Feed `Summary` and `Content` fields may contain HTML. Sanitize before rendering in a UI.

## Dependency injection

```csharp
builder.Services.AddSimpleRss(options => options with { MaxConcurrentRequests = 8 });

public class FeedService(FeedReader reader)
{
    public Task<Feed> LoadAsync(string url) => reader.GetFeedAsync(url);
}
```

Response caching is **opt-in** when using dependency injection:

```csharp
builder.Services.AddSimpleRss(options => options with { EnableResponseCache = true });
```

## Parse local XML

```csharp
var feed = FeedReader.Parse(File.ReadAllText("feed.xml"));

await using var stream = File.OpenRead("feed.xml");
var feedFromStream = await FeedReader.ParseAsync(stream);
```

## Read multiple feeds

`GetFeedsAsync` returns per-feed results and supports partial success:

```csharp
using var reader = new FeedReader();

var results = await reader.GetFeedsAsync([
    "https://example.com/feed-a.rss",
    "https://example.com/feed-b.rss"
]);

foreach (var result in results.Where(result => result.IsSuccess))
{
    Console.WriteLine(result.Feed!.Title);
}
```

Combine items across feeds:

```csharp
using var reader = new FeedReader(new FeedReaderOptions
{
    InterleaveMultipleFeeds = true
});

var items = await reader.GetCombinedItemsAsync([
    "https://example.com/feed-a.rss",
    "https://example.com/feed-b.rss"
]);
```

## Discover feeds from a website

```csharp
using var discovery = new FeedDiscovery(new HttpClient());
var feedUrl = await discovery.DiscoverFeedUrlAsync("https://example.com/blog");

using var reader = new FeedReader();
var feed = await reader.DiscoverAndGetFeedAsync("https://example.com/blog");
```

## Conditional HTTP caching

```csharp
var cache = new InMemoryFeedResponseCache();
using var reader = new FeedReader(new FeedReaderOptions
{
    ResponseCache = cache
});

var feed = await reader.GetFeedAsync("https://example.com/feed.rss");
```

## Supported formats

- RSS 2.0
- Atom 1.0
- RSS 1.0 / RDF

## License

Apache-2.0 — see [LICENSE](LICENSE).
