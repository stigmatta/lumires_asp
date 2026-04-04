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
        [AliasAs("page")] int page,
        CancellationToken ct);
}