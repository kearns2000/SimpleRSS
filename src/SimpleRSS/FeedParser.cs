using System.Xml;
using System.Xml.Linq;

namespace SimpleRSS;

internal static class FeedParser
{
    private static readonly XNamespace Atom = "http://www.w3.org/2005/Atom";
    private static readonly XNamespace Rss10 = "http://purl.org/rss/1.0/";
    private static readonly XNamespace Rdf = "http://www.w3.org/1999/02/22-rdf-syntax-ns#";

    internal static Feed Parse(string xml, FeedReaderOptions? options = null, string? sourceUrl = null)
    {
        options ??= new FeedReaderOptions();

        if (string.IsNullOrWhiteSpace(xml))
        {
            throw new FeedParseException("Feed content is empty.", sourceUrl);
        }

        using var stringReader = new StringReader(xml);
        return Parse(stringReader, options, sourceUrl);
    }

    internal static Feed Parse(Stream stream, FeedReaderOptions? options = null, string? sourceUrl = null)
    {
        Guard.NotNull(stream, nameof(stream));
        options ??= new FeedReaderOptions();

        XDocument document;
        try
        {
            using var xmlReader = XmlReader.Create(stream, CreateXmlReaderSettings(options.MaxXmlCharacterCount));
            document = XDocument.Load(xmlReader, LoadOptions.None);
        }
        catch (Exception ex) when (ex is XmlException or InvalidOperationException)
        {
            throw new FeedParseException("Feed content is not valid XML.", ex, sourceUrl);
        }

        return ParseDocument(document, options, sourceUrl);
    }

    private static Feed Parse(TextReader textReader, FeedReaderOptions options, string? sourceUrl)
    {
        XDocument document;
        try
        {
            using var xmlReader = XmlReader.Create(textReader, CreateXmlReaderSettings(options.MaxXmlCharacterCount));
            document = XDocument.Load(xmlReader, LoadOptions.None);
        }
        catch (Exception ex) when (ex is XmlException or InvalidOperationException)
        {
            throw new FeedParseException("Feed content is not valid XML.", ex, sourceUrl);
        }

        return ParseDocument(document, options, sourceUrl);
    }

    private static Feed ParseDocument(XDocument document, FeedReaderOptions options, string? sourceUrl)
    {
        var root = document.Root;
        if (root is null)
        {
            throw new FeedParseException("Feed XML has no root element.", sourceUrl);
        }

        return root.Name.LocalName switch
        {
            "rss" => ParseRss(root, options, sourceUrl),
            "feed" => ParseAtom(root, options, sourceUrl),
            "RDF" => ParseRdf(root, options, sourceUrl),
            _ => throw new FeedParseException($"Unsupported feed root element '{root.Name.LocalName}'. Expected 'rss', 'feed', or 'RDF'.", sourceUrl)
        };
    }

    private static XmlReaderSettings CreateXmlReaderSettings(long maxXmlCharacterCount) => new()
    {
        DtdProcessing = DtdProcessing.Prohibit,
        XmlResolver = null,
        MaxCharactersInDocument = maxXmlCharacterCount
    };

    private static Feed ParseRss(XElement root, FeedReaderOptions options, string? sourceUrl)
    {
        var channel = root.Element("channel") ?? root;
        return new Feed
        {
            Title = Normalize(channel.ElementValue("title"), options),
            Link = channel.RssLink(),
            Description = Normalize(channel.ElementValue("description"), options),
            SourceUrl = sourceUrl,
            Items = channel.Elements("item").Select(item => ParseRssItem(item, options)).ToList()
        };
    }

    private static FeedItem ParseRssItem(XElement item, FeedReaderOptions options)
    {
        var summary = Normalize(item.ElementValue("description"), options);
        var content = Normalize(item.ElementValueByLocalName("encoded"), options);
        var pubDateRaw = item.ElementValue("pubDate");

        return new FeedItem
        {
            Title = Normalize(item.ElementValue("title"), options),
            Link = item.RssItemLink(),
            Summary = summary,
            Content = content,
            PublishedDateRaw = pubDateRaw,
            Published = FeedDateParser.TryParse(pubDateRaw),
            UpdatedDateRaw = item.ElementValueByLocalName("date"),
            Updated = FeedDateParser.TryParse(item.ElementValueByLocalName("date")),
            Id = item.ElementValue("guid"),
            Author = Normalize(item.ElementValue("author") ?? item.ElementValueByLocalName("creator"), options),
            Categories = item.ParseCategories(),
            Enclosures = item.ParseEnclosures(),
            ImageUrl = item.ParseMediaThumbnail() ?? item.ElementValue("image")
        };
    }

    private static Feed ParseAtom(XElement root, FeedReaderOptions options, string? sourceUrl)
    {
        return new Feed
        {
            Title = Normalize(root.ElementValue(Atom + "title"), options),
            Link = root.AtomFeedLink(),
            Description = Normalize(root.ElementValue(Atom + "subtitle"), options),
            SourceUrl = sourceUrl,
            Items = root.Elements(Atom + "entry").Select(entry => ParseAtomEntry(entry, options)).ToList()
        };
    }

    private static FeedItem ParseAtomEntry(XElement entry, FeedReaderOptions options)
    {
        var publishedRaw = entry.ElementValue(Atom + "published");
        var updatedRaw = entry.ElementValue(Atom + "updated");
        var summary = Normalize(entry.ElementValue(Atom + "summary"), options);
        var content = Normalize(entry.ElementValue(Atom + "content"), options);

        return new FeedItem
        {
            Title = Normalize(entry.ElementValue(Atom + "title"), options),
            Link = entry.AtomEntryLink(),
            Summary = summary,
            Content = content,
            PublishedDateRaw = publishedRaw ?? updatedRaw,
            Published = FeedDateParser.TryParse(publishedRaw ?? updatedRaw),
            UpdatedDateRaw = updatedRaw,
            Updated = FeedDateParser.TryParse(updatedRaw),
            Id = entry.ElementValue(Atom + "id"),
            Author = Normalize(entry.ElementValue(Atom + "author", Atom + "name"), options),
            Categories = entry.ParseAtomCategories(),
            Enclosures = entry.ParseAtomEnclosures(),
            ImageUrl = entry.ParseMediaThumbnail()
        };
    }

    private static Feed ParseRdf(XElement root, FeedReaderOptions options, string? sourceUrl)
    {
        var channel = root.Element(Rss10 + "channel");
        return new Feed
        {
            Title = Normalize(channel?.ElementValue(Rss10 + "title"), options),
            Link = channel?.ElementValue(Rss10 + "link"),
            Description = Normalize(channel?.ElementValue(Rss10 + "description"), options),
            SourceUrl = sourceUrl,
            Items = root.Descendants(Rss10 + "item").Select(item => ParseRdfItem(item, options)).ToList()
        };
    }

    private static FeedItem ParseRdfItem(XElement item, FeedReaderOptions options)
    {
        var pubDateRaw = item.ElementValue(Rss10 + "date") ?? item.ElementValueByLocalName("date");
        return new FeedItem
        {
            Title = Normalize(item.ElementValue(Rss10 + "title"), options),
            Link = item.ElementValue(Rss10 + "link"),
            Summary = Normalize(item.ElementValue(Rss10 + "description"), options),
            PublishedDateRaw = pubDateRaw,
            Published = FeedDateParser.TryParse(pubDateRaw),
            Id = item.Attribute(Rdf + "about")?.Value ?? item.ElementValue(Rss10 + "about"),
            Categories = item.Elements(Rss10 + "category")
                .Select(category => category.Attribute(Rdf + "resource")?.Value
                    ?? category.Attribute("resource")?.Value
                    ?? category.Value)
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Select(value => value!)
                .ToList()
        };
    }

    private static string? Normalize(string? value, FeedReaderOptions options) =>
        FeedHtmlUtility.Normalize(value, options.DecodeHtmlEntities, options.StripHtmlFromText);

    private static string? ElementValue(this XElement parent, XName name, XName? childName = null)
    {
        var element = parent.Element(name);
        if (element is null)
        {
            return null;
        }

        return childName is null ? element.Value : element.Element(childName)?.Value;
    }

    private static string? ElementValueByLocalName(this XElement parent, string localName) =>
        parent.Elements().FirstOrDefault(element => element.Name.LocalName == localName)?.Value;

    private static string? ParseMediaThumbnail(this XElement element)
    {
        var thumbnail = element.Elements().FirstOrDefault(child => child.Name.LocalName == "thumbnail");
        return thumbnail?.Attribute("url")?.Value;
    }

    private static string? RssLink(this XElement channel)
    {
        var link = channel.ElementValue("link");
        if (!string.IsNullOrWhiteSpace(link))
        {
            return link;
        }

        var atomLink = channel.Elements(Atom + "link")
            .FirstOrDefault(element => HasRelationship(element.Attribute("rel")?.Value, "alternate"));
        return atomLink?.Attribute("href")?.Value;
    }

    private static string? RssItemLink(this XElement item)
    {
        var link = item.ElementValue("link");
        if (!string.IsNullOrWhiteSpace(link))
        {
            return link;
        }

        var guid = item.Element("guid");
        if (guid is not null)
        {
            var isPermaLink = guid.Attribute("isPermaLink")?.Value;
            if (!string.Equals(isPermaLink, "false", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrWhiteSpace(guid.Value))
            {
                return guid.Value;
            }
        }

        return null;
    }

    private static string? AtomFeedLink(this XElement feed)
    {
        var links = feed.Elements(Atom + "link").ToList();
        var alternate = links.FirstOrDefault(link => HasRelationship(link.Attribute("rel")?.Value, "alternate"));
        return (alternate ?? links.FirstOrDefault())?.Attribute("href")?.Value;
    }

    private static string? AtomEntryLink(this XElement entry)
    {
        var links = entry.Elements(Atom + "link").ToList();
        if (links.Count == 0)
        {
            return null;
        }

        var alternate = links.FirstOrDefault(link => HasRelationship(link.Attribute("rel")?.Value, "alternate"));
        return (alternate ?? links[0]).Attribute("href")?.Value;
    }

    private static bool HasRelationship(string? relValue, string relationship)
    {
        if (string.IsNullOrWhiteSpace(relValue))
        {
            return relationship.Equals("alternate", StringComparison.OrdinalIgnoreCase);
        }

        return relValue
            .Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Any(token => token.Equals(relationship, StringComparison.OrdinalIgnoreCase));
    }

    private static IReadOnlyList<string> ParseCategories(this XElement item) =>
        item.Elements("category")
            .Select(category => category.Attribute("term")?.Value ?? category.Value)
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(value => value!)
            .ToList();

    private static IReadOnlyList<string> ParseAtomCategories(this XElement entry) =>
        entry.Elements(Atom + "category")
            .Select(category => category.Attribute("term")?.Value ?? category.Value)
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(value => value!)
            .ToList();

    private static IReadOnlyList<FeedEnclosure> ParseEnclosures(this XElement item)
    {
        var enclosures = new List<FeedEnclosure>();

        foreach (var enclosure in item.Elements("enclosure"))
        {
            var url = enclosure.Attribute("url")?.Value;
            if (string.IsNullOrWhiteSpace(url))
            {
                continue;
            }

            enclosures.Add(new FeedEnclosure
            {
                Url = url!,
                Type = enclosure.Attribute("type")?.Value,
                Length = long.TryParse(enclosure.Attribute("length")?.Value, out var length) ? length : null
            });
        }

        return enclosures;
    }

    private static IReadOnlyList<FeedEnclosure> ParseAtomEnclosures(this XElement entry) =>
        entry.Elements(Atom + "link")
            .Where(link => string.Equals(link.Attribute("rel")?.Value, "enclosure", StringComparison.OrdinalIgnoreCase))
            .Select(link => new FeedEnclosure
            {
                Url = link.Attribute("href")?.Value ?? string.Empty,
                Type = link.Attribute("type")?.Value,
                Length = long.TryParse(link.Attribute("length")?.Value, out var length) ? length : null
            })
            .Where(enclosure => !string.IsNullOrWhiteSpace(enclosure.Url))
            .ToList();
}
