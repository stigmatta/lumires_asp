using System.Net;
using Ardalis.Result;
using Infrastructure.Exceptions;
using lumires.Core.Abstractions.Services;
using lumires.Core.Constants;
using lumires.Core.Models;
using ZiggyCreatures.Caching.Fusion;

namespace Infrastructure.Services.Watchmode;

public sealed class WatchmodeService(IWatchmodeApi watchmodeApi, IFusionCache cache) : IStreamingService
{
    public async Task<Result<List<FilmSource>>> GetSourcesAsync(
        int tmdbId,
        CancellationToken ct,
        string region = "US")
    {
        var sourcesKey = CacheKeys.FilmSources(tmdbId, region);

        try
        {
            var sources = await cache.GetOrSetAsync<List<FilmSource>>(
                sourcesKey,
                async (_, token) =>
                {
                    var watchmodeIdResult = await GetWatchmodeIdAsync(tmdbId, token);

                    if (!watchmodeIdResult.IsSuccess)
                        throw new WatchmodeException(watchmodeIdResult.Status, "Watchmode API error");

                    if (watchmodeIdResult.Value == 0)
                        return [];

                    var externalSources = await watchmodeApi
                        .GetSourcesAsync(watchmodeIdResult.Value, region, token);

                    if (!externalSources.IsSuccessful || externalSources.Content is null)
                        return [];

                    return externalSources.Content
                        .Select(dto => new FilmSource(
                            tmdbId,
                            dto.Name,
                            dto.Type,
                            dto.WebUrl,
                            dto.Format,
                            dto.Price))
                        .ToList();
                },
                options => options.SetDuration(CacheDuration.Medium).SetFailSafe(true),
                ct
            );

            return Result.Success(sources);
        }
        catch (WatchmodeException ex)
        {
            return ex.Status switch
            {
                ResultStatus.Unauthorized => Result.Unauthorized(),
                ResultStatus.NotFound => Result.NotFound(),
                _ => Result.Error("Watchmode API error")
            };
        }
    }

    private async Task<Result<int>> GetWatchmodeIdAsync(int tmdbId, CancellationToken ct)
    {
        var idMapKey = CacheKeys.FilmSourceExternalId(tmdbId);

        try
        {
            var watchmodeId = await cache.GetOrSetAsync<int?>(
                idMapKey,
                async (_, token) =>
                {
                    var searchResponse = await watchmodeApi.SearchByTmdbIdAsync(tmdbId, token);

                    if (searchResponse is { IsSuccessful: true, Content: not null })
                        return searchResponse.Content.TitleResults is { Count: > 0 }
                            ? searchResponse.Content.TitleResults[0].Id
                            : 0;

                    throw searchResponse.StatusCode switch
                    {
                        HttpStatusCode.Unauthorized =>
                            new WatchmodeException(ResultStatus.Unauthorized, "Unauthorized"),
                        HttpStatusCode.NotFound => new WatchmodeException(ResultStatus.NotFound, "Not found"),
                        _ => new WatchmodeException(ResultStatus.Error, "Watchmode API error")
                    };
                },
                options => options.SetDuration(CacheDuration.Medium).SetFailSafe(true),
                ct
            );

            return Result.Success(watchmodeId ?? 0);
        }
        catch (WatchmodeException ex)
        {
            return ex.Status switch
            {
                ResultStatus.Unauthorized => Result.Unauthorized(),
                ResultStatus.NotFound => Result.NotFound(),
                _ => Result.Error("Watchmode API error")
            };
        }
    }
}