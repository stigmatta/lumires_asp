using Refit;

namespace Infrastructure.Services.Watchmode;

public interface IWatchmodeApi
{
    [Get("/search/")]
    Task<ApiResponse<WatchmodeSearchResponse>> SearchByTmdbIdAsync(
        [AliasAs("search_value")] int searchValue,
        CancellationToken ct,
        [AliasAs("search_field")] string searchField = "tmdb_movie_id");

    [Get("/title/{watchmodeId}/sources/")]
    Task<ApiResponse<List<WatchmodeSourceResponse>>> GetSourcesAsync(
        int watchmodeId,
        string regions,
        CancellationToken ct = default);
}