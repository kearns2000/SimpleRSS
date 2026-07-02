using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace SimpleRSS;

/// <summary>
/// Dependency injection helpers for SimpleRSS.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers <see cref="FeedReader"/>, <see cref="FeedDiscovery"/>, and a named HTTP client.
    /// </summary>
    public static IServiceCollection AddSimpleRss(
        this IServiceCollection services,
        Func<FeedReaderOptions, FeedReaderOptions>? configureOptions = null)
    {
        Guard.NotNull(services, nameof(services));

        var options = configureOptions?.Invoke(new FeedReaderOptions()) ?? new FeedReaderOptions();

        services.TryAddSingleton(options);

        if (options.EnableResponseCache)
        {
            services.TryAddSingleton<IFeedResponseCache, InMemoryFeedResponseCache>();
        }

        services.AddHttpClient(SimpleRssHttpClientNames.FeedReader, client =>
            {
                client.Timeout = Timeout.InfiniteTimeSpan;
                client.DefaultRequestHeaders.UserAgent.ParseAdd(options.UserAgent);
            });

        services.TryAddSingleton<FeedReader>(serviceProvider =>
        {
            var httpClientFactory = serviceProvider.GetRequiredService<System.Net.Http.IHttpClientFactory>();
            var configuredOptions = serviceProvider.GetRequiredService<FeedReaderOptions>();
            var readerOptions = ResolveReaderOptions(serviceProvider, configuredOptions);
            var httpClient = httpClientFactory.CreateClient(SimpleRssHttpClientNames.FeedReader);
            return new FeedReader(httpClient, readerOptions);
        });

        services.TryAddSingleton<FeedDiscovery>(serviceProvider =>
        {
            var httpClientFactory = serviceProvider.GetRequiredService<System.Net.Http.IHttpClientFactory>();
            var configuredOptions = serviceProvider.GetRequiredService<FeedReaderOptions>();
            var httpClient = httpClientFactory.CreateClient(SimpleRssHttpClientNames.FeedReader);
            return new FeedDiscovery(httpClient, configuredOptions);
        });

        return services;
    }

    private static FeedReaderOptions ResolveReaderOptions(IServiceProvider serviceProvider, FeedReaderOptions configuredOptions)
    {
        if (!configuredOptions.EnableResponseCache || configuredOptions.ResponseCache is not null)
        {
            return configuredOptions;
        }

        var cache = serviceProvider.GetService<IFeedResponseCache>();
        return cache is null
            ? configuredOptions
            : configuredOptions with { ResponseCache = cache };
    }
}

internal static class SimpleRssHttpClientNames
{
    internal const string FeedReader = "SimpleRSS.FeedReader";
}
