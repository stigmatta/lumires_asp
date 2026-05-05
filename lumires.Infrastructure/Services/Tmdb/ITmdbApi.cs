using Refit;

namespace Infrastructure.Services.Tmdb;

public interface ITmdbApi
{
    [Get("/movie/{movieId}?append_to_response=credits,videos")]
    Task<ApiResponse<TmdbMovieResponse>> GetMovieAsync(int movieId, [AliasAs("language")] string lang,
        CancellationToken ct);

    [Get("/trending/movie/week")]
    Task<ApiResponse<TmdbPagedResponse<TmdbMovieShortResponse>>> GetTrendingMoviesAsync(CancellationToken ct);

    [Get("/movie/popular")]
    Task<ApiResponse<TmdbPagedResponse<TmdbMovieShortResponse>>> GetPopularMoviesAsync(
        [AliasAs("sort_by")] int page,
        CancellationToken ct);

    [Get("/discover/movie")]
    Task<ApiResponse<TmdbPagedResponse<TmdbMovieShortResponse>>> GetRecentReleasesAsync(
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
}