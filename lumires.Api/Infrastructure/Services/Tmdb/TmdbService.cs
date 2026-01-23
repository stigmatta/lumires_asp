using lumires.Api.Core.Abstractions;
using lumires.Api.Core.Models;

namespace lumires.Api.Infrastructure.Services.Tmdb;

public sealed class TmdbService(ITmdbApi tmdbApi, ICurrentUserService currentUserService) : IExternalMovieService
{
    public async Task<MovieImportResult?> GetMovieDetailsAsync(int movieId, CancellationToken ct = default)
    {
        var lang = currentUserService.LangCulture;
        var response = await tmdbApi.GetMovieAsync(movieId, lang, ct);

        if (!response.IsSuccessStatusCode || response.Content == null) return null;

        var result = MapToDomain(response.Content);

        if ((!string.IsNullOrWhiteSpace(result.Overview) && result.TrailerUrl != null) || lang == "en-US")
            return result;

        var fallbackResponse = await tmdbApi.GetMovieAsync(movieId, "en-US", ct);
        if (fallbackResponse.Content == null) return result;

        var fallback = MapToDomain(fallbackResponse.Content);

        return result with 
        { 
            Overview = string.IsNullOrWhiteSpace(result.Overview) ? fallback.Overview : result.Overview,
            TrailerUrl = result.TrailerUrl ?? fallback.TrailerUrl
        };
    }

    private static MovieImportResult MapToDomain(TmdbMovieResponse tmdb)
    {
        var trailerKey = tmdb.Videos?.Results?
            .FirstOrDefault(v => v is { Type: "Trailer", Site: "YouTube" })?.Key;

        return new MovieImportResult(
            tmdb.Id,
            tmdb.Title,
            tmdb.Overview,
            tmdb.PosterPath,
            tmdb.ReleaseDate,
            trailerKey != null ? new Uri($"https://www.youtube.com/watch?v={trailerKey}") : null
        );
    }
}