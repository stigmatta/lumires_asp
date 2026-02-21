using Core.Abstractions.Services;
using Core.Constants;
using Core.Models;
using ZiggyCreatures.Caching.Fusion;

namespace Infrastructure.Services.Watchmode;

public sealed class WatchmodeService(IWatchmodeApi watchmodeApi, IFusionCache cache) : IStreamingService
{
    public async Task<List<MovieSource>> GetSourcesAsync(int tmdbId, CancellationToken ct, string region = "US")
    {
        var sourcesKey = CacheKeys.MovieSources(tmdbId, region);

        return await cache.GetOrSetAsync(sourcesKey, async token =>
        {
            var watchmodeId = await GetWatchmodeIdAsync(tmdbId, token);

            if (watchmodeId == 0) return [];

            var externalSources = await watchmodeApi.GetSourcesAsync(watchmodeId, region, token);

            return externalSources.Select(dto => new MovieSource(
                tmdbId,
                dto.Name,
                dto.Type,
                dto.WebUrl,
                dto.Format,
                dto.Price
            )).ToList();
        }, token: ct);
    }

    private async Task<int> GetWatchmodeIdAsync(int tmdbId, CancellationToken ct)
    {
        var idMapKey = CacheKeys.MovieSourceExternalId(tmdbId);

        return await cache.GetOrSetAsync(idMapKey, async token =>
            {
                var searchResponse = await watchmodeApi.SearchByTmdbIdAsync(tmdbId, token);

                return searchResponse.TitleResults is { Count: > 0 } ? searchResponse.TitleResults[0].Id : 0;
            },
            new FusionCacheEntryOptions { Duration = TimeSpan.FromHours(1) }, ct);
    }
}