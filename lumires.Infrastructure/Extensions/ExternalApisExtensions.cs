using System.Diagnostics;
using System.Net;
using System.Text.Json;
using Infrastructure.Options;
using Infrastructure.Services.Tmdb;
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
                WriteIndented = true
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