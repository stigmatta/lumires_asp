using System.Net;
using Ardalis.Result;
using lumires.Core.Abstractions.Data;
using lumires.Core.Abstractions.Services;
using lumires.Core.Constants;
using lumires.Core.Models;
using lumires.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services.Tmdb;

public sealed class TmdbService(ITmdbApi tmdbApi, IAppDbContext db) : IExternalMovieService
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

    public async Task<Result> SyncTrendingMoviesAsync(CancellationToken ct)
    {
        var trendingResponse = await tmdbApi.GetTrendingMoviesAsync(ct);

        if (trendingResponse.StatusCode == HttpStatusCode.Unauthorized)
            return Result.Unauthorized();

        if (!trendingResponse.IsSuccessStatusCode || trendingResponse.Content is null)
            return Result.Error("Failed to fetch trending movies from TMDB");

        foreach (var batch in trendingResponse.Content.Results.Chunk(10))
        {
            var tmdbIds = batch.Select(m => m.Id).ToList();

            var existingIds = await db.Movies
                .Where(m => tmdbIds.Contains(m.ExternalId))
                .Select(m => m.ExternalId)
                .ToHashSetAsync(ct);

            var tasks = batch.Select(async movie =>
            {
                var enTask = tmdbApi.GetMovieAsync(movie.Id, "en-US", ct);
                var ukTask = tmdbApi.GetMovieAsync(movie.Id, "uk-UA", ct);
                await Task.WhenAll(enTask, ukTask);
                return (en: await enTask, uk: await ukTask, movie.Id);
            });

            var results = await Task.WhenAll(tasks);

            foreach (var (en, uk, tmdbId) in results)
            {
                if (!en.IsSuccessStatusCode || !uk.IsSuccessStatusCode) continue;

                if (existingIds.Contains(tmdbId))
                {
                    var localizations = await db.MovieLocalizations
                        .Where(l => l.Movie.ExternalId == tmdbId)
                        .ToListAsync(ct);

                    foreach (var loc in localizations)
                    {
                        var source = loc.LanguageCode == "en" ? en.Content : uk.Content;
                        loc.Update(source!.Title, source.Overview);
                    }
                }
                else
                {
                    await db.Movies.AddAsync(ToMovie(en.Content!, uk.Content!), ct);
                }
            }

            await db.SaveChangesAsync(ct);
        }
        return Result.NoContent();
    }
    public async Task<Result> SyncPopularMoviesAsync(CancellationToken ct)
    {
        const int targetNewMovies = 40;
        var newMoviesCount = 0;
        var page = 1;

        while (newMoviesCount < targetNewMovies)
        {
            var popularResponse = await tmdbApi.GetPopularMoviesAsync(page, ct);

            if (popularResponse.StatusCode == HttpStatusCode.Unauthorized)
                return Result.Unauthorized();

            if (!popularResponse.IsSuccessStatusCode || popularResponse.Content is null)
                return Result.Error("Failed to fetch popular movies from TMDB");

            if (page > popularResponse.Content.TotalPages)
                break;

            var tmdbIds = popularResponse.Content.Results.Select(m => m.Id).ToList();

            var existingIds = await db.Movies
                .Where(m => tmdbIds.Contains(m.ExternalId))
                .Select(m => m.ExternalId)
                .ToHashSetAsync(ct);

            var newMovies = popularResponse.Content.Results
                .Where(m => !existingIds.Contains(m.Id))
                .ToList();

            foreach (var batch in newMovies.Chunk(10))
            {
                var tasks = batch.Select(async movie =>
                {
                    var enTask = tmdbApi.GetMovieAsync(movie.Id, "en-US", ct);
                    var ukTask = tmdbApi.GetMovieAsync(movie.Id, "uk-UA", ct);
                    await Task.WhenAll(enTask, ukTask);
                    return (en: await enTask, uk: await ukTask);
                });

                var results = await Task.WhenAll(tasks);

                foreach (var (en, uk) in results)
                {
                    if (!en.IsSuccessStatusCode || !uk.IsSuccessStatusCode) continue;

                    await db.Movies.AddAsync(ToMovie(en.Content!, uk.Content!), ct);
                    newMoviesCount++;
                }

                await db.SaveChangesAsync(ct);
            }

            page++;
        }

        return Result.NoContent();
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
            tmdb.VoteAverage,
            tmdb.VoteCount,
            tmdb.Popularity,
            tmdb.BackdropPath,
            tmdb.ReleaseDate,
            trailerKey
        );
    }
    
    private static Movie ToMovie(TmdbMovieResponse en, TmdbMovieResponse uk)
    {
        var trailerKey = en.Videos?.Results
            .FirstOrDefault(v => v is { Type: "Trailer", Site: "YouTube" })?.Key;

        var movie = new Movie(
            externalId: en.Id,
            releaseDate: en.ReleaseDate,
            posterPath: en.PosterPath,
            voteAverage: en.VoteAverage,
            voteCount: en.VoteCount,
            popularity: en.Popularity,
            backdropPath: en.BackdropPath,
            trailerUrl: trailerKey
        );

        movie.AddLocalization(new MovieLocalization("en-US", en.Title, en.Overview));
        movie.AddLocalization(new MovieLocalization("uk-UA", uk.Title, uk.Overview));

        return movie;
    }
}