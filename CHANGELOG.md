# Changelog

All notable changes to this project are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [2.1.2] - 2026-07-06

### Changed
- Expanded NuGet search metadata with comprehensive tags, title, description, and README keywords for RSS/Atom feed reader and parser discoverability

## [2.1.1] - 2026-07-02

### Added
- SourceLink and symbol packages for NuGet debugging
- `MaxResponseBytes` and `EnableResponseCache` options
- Bounded in-memory cache eviction in `InMemoryFeedResponseCache`

### Changed
- HTTP responses are size-limited before XML parsing
- Feed XML is parsed from streams to preserve declared encodings
- `RequestTimeout` is enforced for shared `HttpClient` instances
- Response caching in DI is now opt-in via `EnableResponseCache`
- Public DTO types are now `record`s for consistent equality semantics
- Improved feed discovery, RDF category parsing, and RFC 822 date parsing

### Fixed
- Caller cancellation is no longer reported as a timeout
- `ParseAsync` honors cancellation tokens
- Tests now run on both `net8.0` and `net10.0`

## [2.1.0] - 2026-07-02

### Added
- `Feed` model with channel metadata
- Typed `DateTimeOffset` dates on `FeedItem`
- Separate `Summary` and `Content` fields
- Categories, enclosures, and image URL support
- RSS 2.0, Atom 1.0, and RSS 1.0/RDF parsing
- Secure XML parsing with size limits
- HTML entity decoding and optional HTML stripping
- Parallel multi-feed fetching with concurrency limits
- `FeedResult` for per-feed success/failure handling
- Conditional HTTP requests via `IFeedResponseCache`
- Feed auto-discovery from HTML pages
- Dependency injection via `AddSimpleRss()`
- Unit tests and GitHub Actions CI

### Changed
- Modern async API (`FeedReader`, `FeedItem`, etc.)
- Targets .NET 8 and .NET 10
- Improved error handling with `FeedParseException` and `FeedHttpException`

### Removed
- Legacy .NET Framework API (`RSS.getFeed`, `feed` class, etc.)

## [2.0.0] - 2026-07-02

### Added
- Initial modern rewrite for .NET

## [1.0.0.7] - 2018-11-19

### Added
- Ability to parse multiple feeds

## [1.0.0] - 2018-05-02

### Added
- Initial release on NuGet
- Basic RSS feed parsing for .NET Framework 4.5.2

[Unreleased]: https://github.com/kearns2000/SimpleRSS/compare/v2.1.2...HEAD
[2.1.2]: https://github.com/kearns2000/SimpleRSS/compare/v2.1.1...v2.1.2
[2.1.1]: https://github.com/kearns2000/SimpleRSS/compare/v2.1.0...v2.1.1
[2.1.0]: https://github.com/kearns2000/SimpleRSS/compare/v2.0.0...v2.1.0
[2.0.0]: https://github.com/kearns2000/SimpleRSS/compare/v1.0.0.7...v2.0.0
[1.0.0.7]: https://www.nuget.org/packages/SimpleRSS/1.0.0.7
[1.0.0]: https://www.nuget.org/packages/SimpleRSS/1.0.0
