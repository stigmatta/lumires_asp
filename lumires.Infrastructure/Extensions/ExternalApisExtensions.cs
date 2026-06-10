using System.Diagnostics;
using System.Net;
using System.Text.Json;
using Infrastructure.Options;
using Infrastructure.Services.Tmdb;
using Infrastructure.Services.Tmdb.TmdbAwards;
using Infrastructure.Services.Tmdb.TmdbFilms;
using Infrastructure.Services.Tmdb.TmdbPerson;
using Infrastructure.Services.Tmdb.TmdbSearch;
using Infrastructure.Services.Watchmode;
using lumires.Core.Abstractions.Services;
using Microsoft.Extensions.Options;
using Polly;
using Refit;

namespace Infrastructure.Extensions;

internal static class ExternalApiExtensions
{
    public static IServiceCollection AddExternalApis(this IServiceCollection services, IConfiguration config)
    {
        Debug.Assert(config != null, nameof(config) + " != null");

        var refitSettings = new RefitSettings
        {
            ContentSerializer = new SystemTextJsonContentSerializer(new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
                WriteIndented = true,
                Converters = { new EmptyStringDateOnlyConverter() }
            })
        };

        // TMDB
        services.Configure<TmdbConfig>(config.GetSection(TmdbConfig.Section));
        services.AddTransient<TmdbAuthHandler>();
        services.AddRefitClient<ITmdbApi>(refitSettings)
            .ConfigureHttpClient((sp, client) =>
                client.BaseAddress = sp.GetRequiredService<IOptions<TmdbConfig>>().Value.BaseUrl)
            .ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler
            {
                AutomaticDecompression = DecompressionMethods.All,
                PooledConnectionLifetime = TimeSpan.FromMinutes(15)
            })
            .SetHandlerLifetime(Timeout.InfiniteTimeSpan)
            .AddHttpMessageHandler<TmdbAuthHandler>()
            .AddTransientHttpErrorPolicy(p => p.WaitAndRetryAsync(3, _ => TimeSpan.FromMilliseconds(600)));
        services.AddScoped<IExternalFilmService, TmdbFilmService>();
        services.AddScoped<IExternalPersonService, TmdbPersonService>();
        services.AddScoped<ISearchService, TmdbSearchService>();

        // TMDB awards (scraped from the public website — no awards API exists)
        services.AddHttpClient<IExternalAwardsService, TmdbAwardsService>((sp, client) =>
            {
                var tmdb = sp.GetRequiredService<IOptions<TmdbConfig>>().Value;
                client.BaseAddress = new Uri(tmdb.SiteUrl.AbsoluteUri.TrimEnd('/') + "/");
                client.DefaultRequestHeaders.AcceptLanguage.ParseAdd("en-US");
                client.DefaultRequestHeaders.UserAgent.ParseAdd(
                    "Mozilla/5.0 (compatible; LumiresBot/1.0; +https://themoviedb.org)");
                client.Timeout = TimeSpan.FromSeconds(10);
            })
            .ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler
            {
                AutomaticDecompression = DecompressionMethods.All,
                PooledConnectionLifetime = TimeSpan.FromMinutes(15)
            })
            .AddTransientHttpErrorPolicy(p => p.WaitAndRetryAsync(2, _ => TimeSpan.FromMilliseconds(500)));

        // Watchmode
        services.Configure<WatchmodeOptions>(config.GetSection(WatchmodeOptions.SectionName));
        services.AddTransient<WatchmodeAuthHandler>();
        services.AddRefitClient<IWatchmodeApi>(refitSettings)
            .ConfigureHttpClient((sp, client) =>
                client.BaseAddress = sp.GetRequiredService<IOptions<WatchmodeOptions>>().Value.BaseUrl)
            .ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler
            {
                AutomaticDecompression = DecompressionMethods.All,
                PooledConnectionLifetime = TimeSpan.FromMinutes(15)
            })
            .SetHandlerLifetime(Timeout.InfiniteTimeSpan)
            .AddHttpMessageHandler<WatchmodeAuthHandler>()
            .AddTransientHttpErrorPolicy(p => p.WaitAndRetryAsync(2, _ => TimeSpan.FromSeconds(1)));
        services.AddScoped<IStreamingService, WatchmodeService>();

        return services;
    }
}