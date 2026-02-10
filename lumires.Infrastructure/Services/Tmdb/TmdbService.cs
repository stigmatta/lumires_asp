using System.Net;
using Ardalis.Result;
using Core.Abstractions.Services;
using Core.Constants;
using Core.Models;

namespace Infrastructure.Services.Tmdb;

public sealed class TmdbService(ITmdbApi tmdbApi) : IExternalMovieService
{
    public async Task<Result<ExternalMovie>> GetMovieDetailsAsync(int movieId, string lang,
        CancellationToken ct = default)
    {
        var response = await tmdbApi.GetMovieAsync(movieId, lang, ct);

        switch (response.StatusCode)
        {
            case HttpStatusCode.Unauthorized:
                return Result.Unauthorized();
            case HttpStatusCode.NotFound:
                return Result.NotFound();
        }

        if (!response.IsSuccessStatusCode || response.Content == null) return Result.Error();

        var result = MapToDomain(response.Content);
        
        const string defLang = LocalizationConstants.DefaultCulture;

        if ((!string.IsNullOrWhiteSpace(result.Overview) && result.TrailerUrl != null) || lang == defLang)
            return result;

        var fallbackResponse = await tmdbApi.GetMovieAsync(movieId, defLang, ct);
        if (fallbackResponse.Content == null) return result;

        var fallback = MapToDomain(fallbackResponse.Content);

        return result with
        {
            Overview = string.IsNullOrWhiteSpace(result.Overview) ? fallback.Overview : result.Overview,
            TrailerUrl = result.TrailerUrl ?? fallback.TrailerUrl
        };
    }

    private static ExternalMovie MapToDomain(TmdbMovieResponse tmdb)
    {
        var trailerKey = tmdb.Videos?.Results?
            .FirstOrDefault(v => v is { Type: "Trailer", Site: "YouTube" })?.Key;

        return new ExternalMovie(
            tmdb.Id,
            tmdb.Title,
            tmdb.Overview,
            tmdb.PosterPath,
            tmdb.ReleaseDate,
            trailerKey != null ? new Uri($"https://www.youtube.com/watch?v={trailerKey}") : null
        );
    }
}