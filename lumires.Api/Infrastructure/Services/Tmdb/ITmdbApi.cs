using lumires.Api.Core.Models;
using Refit;

namespace lumires.Api.Infrastructure.Services.Tmdb;

public interface ITmdbApi
{
    [Get("/movie/{movieId}?append_to_response=credits,videos")]
    Task<ApiResponse<TmdbMovieResponse>> GetMovieAsync(int movieId, [AliasAs("language")] string lang, CancellationToken ct);
}