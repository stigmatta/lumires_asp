using Refit;

namespace lumires.Api.Infrastructure.Services.Watchmode;

public interface IWatchmodeApi
{
    [Get("/search/")]
    Task<WatchmodeSearchResponse> SearchByTmdbIdAsync(
        [AliasAs("search_value")] string searchValue,
        [AliasAs("search_field")] string searchField = "tmdb_movie_id",
        CancellationToken ct = default);

    [Get("/title/{watchmodeId}/sources/")]
    Task<List<WatchmodeSourceResponse>> GetSourcesAsync(
        int watchmodeId,
        string regions,
        CancellationToken ct = default);
}