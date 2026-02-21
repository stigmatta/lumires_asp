using System.Net;
using Ardalis.Result;
using Core.Abstractions.Services;
using Core.Constants;
using Core.Models;
using ZiggyCreatures.Caching.Fusion;

namespace Infrastructure.Services.Watchmode;

public sealed class WatchmodeService(IWatchmodeApi watchmodeApi, IFusionCache cache) : IStreamingService
{
    public async Task<Result<List<MovieSource>>> GetSourcesAsync(
        int tmdbId,
        CancellationToken ct,
        string region = "US")
    {
        var sourcesKey = CacheKeys.MovieSources(tmdbId, region);

        var cachedSources = await cache
            .GetOrDefaultAsync<List<MovieSource>>(sourcesKey, token: ct);

        if (cachedSources is not null)
            return Result.Success(cachedSources);

        var watchmodeIdResult = await GetWatchmodeIdAsync(tmdbId, ct);

        if (!watchmodeIdResult.IsSuccess)
            return watchmodeIdResult.Map(_ => new List<MovieSource>());

        if (watchmodeIdResult.Value == 0)
            return Result.Success(new List<MovieSource>());

        var externalSources = await watchmodeApi
            .GetSourcesAsync(watchmodeIdResult.Value, region, ct);

        if (!externalSources.IsSuccessful || externalSources.Content is null)
            return Result.Success(new List<MovieSource>());

        var mappedSources = externalSources.Content
            .Select(dto => new MovieSource(
                tmdbId,
                dto.Name,
                dto.Type,
                dto.WebUrl,
                dto.Format,
                dto.Price))
            .ToList();

        await cache.SetAsync(sourcesKey, mappedSources, options => options
            .SetDuration(CacheDuration.Medium)
            .SetFailSafe(true), ct);

        return Result.Success(mappedSources);
    }

    private async Task<Result<int>> GetWatchmodeIdAsync(int tmdbId, CancellationToken ct)
    {
        var idMapKey = CacheKeys.MovieSourceExternalId(tmdbId);

        var cachedId = await cache.GetOrDefaultAsync<int>(idMapKey, token: ct);

        if (cachedId is not 0)
            return Result.Success(cachedId);

        var searchResponse = await watchmodeApi.SearchByTmdbIdAsync(tmdbId, ct);

        if (searchResponse is { IsSuccessful: true, Content: not null })
        {
            var watchmodeId = searchResponse.Content.TitleResults is { Count: > 0 }
                ? searchResponse.Content.TitleResults[0].Id
                : 0;

            if (watchmodeId != 0)
                await cache.SetAsync(idMapKey, watchmodeId, options => options
                    .SetDuration(CacheDuration.Medium)
                    .SetFailSafe(true), ct);

            return Result.Success(watchmodeId);
        }

        return searchResponse.StatusCode switch
        {
            HttpStatusCode.Unauthorized => Result.Unauthorized(),
            HttpStatusCode.NotFound => Result.NotFound(),
            _ => Result.Error("Watchmode API error")
        };
    }
}