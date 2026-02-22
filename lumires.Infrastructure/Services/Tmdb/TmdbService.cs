using System.Net;
using Ardalis.Result;
using lumires.Core.Abstractions.Services;
using lumires.Core.Constants;
using lumires.Core.Models;

namespace Infrastructure.Services.Tmdb;

public sealed class TmdbService(ITmdbApi tmdbApi) : IExternalMovieService
{
    public async Task<Result<ExternalMovie>> GetMovieDetailsAsync(int movieId, string lang,
        CancellationToken ct = default)
    {
        var tmdbResponse = await tmdbApi.GetMovieAsync(movieId, lang, ct);

        switch (tmdbResponse.StatusCode)
        {
            case HttpStatusCode.Unauthorized:
                return Result.Unauthorized();
            case HttpStatusCode.NotFound:
                return Result.NotFound();
        }

        if (!tmdbResponse.IsSuccessStatusCode || tmdbResponse.Content == null) return Result.Error();

        var externalMovie = MapToDomain(tmdbResponse.Content);

        const string defLang = LocalizationConstants.DefaultCulture;

        if ((!string.IsNullOrWhiteSpace(externalMovie.Overview) && externalMovie.TrailerUrl != null) || lang == defLang)
            return externalMovie;

        var fallbackResponse = await tmdbApi.GetMovieAsync(movieId, defLang, ct);
        if (fallbackResponse.Content == null) return externalMovie;

        var fallback = MapToDomain(fallbackResponse.Content);

        return externalMovie with
        {
            Overview = string.IsNullOrWhiteSpace(externalMovie.Overview) ? fallback.Overview : externalMovie.Overview,
            TrailerUrl = externalMovie.TrailerUrl ?? fallback.TrailerUrl
        };
    }

    private static ExternalMovie MapToDomain(TmdbMovieResponse tmdb)
    {
        var trailerKey = tmdb.Videos?.Results
            .FirstOrDefault(v => v is { Type: "Trailer", Site: "YouTube" })?.Key;

        return new ExternalMovie(
            tmdb.Id,
            tmdb.Title,
            tmdb.Overview,
            tmdb.PosterPath,
            tmdb.BackdropPath,
            tmdb.ReleaseDate,
            trailerKey
        );
    }
}