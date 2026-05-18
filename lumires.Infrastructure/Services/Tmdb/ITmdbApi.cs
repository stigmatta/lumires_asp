using lumires.Core.Models;
using Refit;

namespace Infrastructure.Services.Tmdb;

public interface ITmdbApi
{
    [Get("/movie/{movieId}?append_to_response=credits,videos")]
    Task<ApiResponse<TmdbMovieResponse>> GetFilmAsync(int movieId, [AliasAs("language")] string lang,
        CancellationToken ct);

    [Get("/movie/{movieId}")]
    Task<ApiResponse<TmdbMovieResponse>> GetFilmShortenedAsync(int movieId, [AliasAs("language")] string lang,
        CancellationToken ct);

    [Get("/trending/movie/week")]
    Task<ApiResponse<PagedResponse<TmdbMovieShortResponse>>> GetTrendingFilmsAsync(CancellationToken ct);

    [Get("/movie/popular")]
    Task<ApiResponse<PagedResponse<TmdbMovieShortResponse>>> GetPopularFilmsAsync(
        [AliasAs("page")] int page,
        CancellationToken ct);

    [Get("/discover/movie")]
    Task<ApiResponse<PagedResponse<TmdbMovieShortResponse>>> GetRecentReleasesAsync(
        [AliasAs("release_date.lte")] string releaseDateLte,
        [AliasAs("release_date.gte")] string releaseDateGte,
        [AliasAs("sort_by")] string sortBy,
        [AliasAs("with_release_type")] string releaseType,
        [AliasAs("region")] string region,
        [AliasAs("include_adult")] bool includeAdult,
        [AliasAs("vote_count.gte")] int minVoteCount,
        [AliasAs("page")] int page,
        CancellationToken ct);

    [Get("/genre/movie/list")]
    Task<ApiResponse<TmdbGenresResponse>> GetGenresAsync(
        [AliasAs("language")] string lang, CancellationToken ct);

    [Get("/movie/{movieId}/similar")]
    Task<ApiResponse<PagedResponse<TmdbMovieShortResponse>>> GetSimilarFilmsAsync(
        int movieId,
        [AliasAs("language")] string lang,
        CancellationToken ct,
        [AliasAs("page")] int page = 1);
    
    [Get("/discover/movie")]
    Task<ApiResponse<PagedResponse<TmdbMovieShortResponse>>> GetTotalFilmsCountAsync(
        [Query] int page = 1,
        [Query] string? language = "en-US",
        [Query] string sortBy = "popularity.desc",
        CancellationToken ct = default);
}