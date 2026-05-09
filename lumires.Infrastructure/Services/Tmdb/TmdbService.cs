using System.Net;
using Ardalis.Result;
using lumires.Core;
using lumires.Core.Abstractions.Data;
using lumires.Core.Abstractions.Services;
using lumires.Core.Constants;
using lumires.Core.Models;
using lumires.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services.Tmdb;

public sealed class TmdbService(ITmdbApi tmdbApi, IAppDbContext db) : IExternalMovieService
{
    private const string DefLang = LocalizationConstants.DefaultCulture;
    private const string EnLang = "en-US";
    private const string UaLang = "uk-UA";

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

        if ((!string.IsNullOrWhiteSpace(externalMovie.Overview) && externalMovie.TrailerUrl != null) || lang == DefLang)
            return externalMovie;

        var fallbackResponse = await tmdbApi.GetMovieAsync(movieId, DefLang, ct);
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

            var results = new List<(int tmdbId, Movie? movie, bool isExisting)>();
            foreach (var m in batch)
            {
                if (existingIds.Contains(m.Id))
                {
                    results.Add((m.Id, null, true));
                    continue;
                }
                var movie = await FetchAndBuildMovieAsync(m.Id, ct);
                results.Add((m.Id, movie, false));
            }

            var existingToUpdate = results
                .Where(r => r.isExisting)
                .Select(r => r.tmdbId)
                .ToList();

            if (existingToUpdate.Count > 0)
                await UpdateLocalizationsAsync(existingToUpdate, ct);

            foreach (var (_, movie, isExisting) in results)
            {
                if (!isExisting && movie is not null)
                    await db.Movies.AddAsync(movie, ct);
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
        const int maxPages = 10;

        while (newMoviesCount < targetNewMovies && page <= maxPages)
        {
            var popularResponse = await tmdbApi.GetPopularMoviesAsync(page, ct);

            if (!popularResponse.IsSuccessStatusCode || popularResponse.Content is null)
                break;

            var data = popularResponse.Content;
            var tmdbIds = data.Results.Select(m => m.Id).ToList();

            var existingIds = await db.Movies
                .Where(m => tmdbIds.Contains(m.ExternalId))
                .Select(m => m.ExternalId)
                .ToHashSetAsync(ct);

            var newMovies = data.Results
                .Where(m => !existingIds.Contains(m.Id))
                .ToList();

            if (newMovies.Count == 0)
            {
                page++;
                continue;
            }

            foreach (var batch in newMovies.Chunk(10))
            {
                var movies = new List<Movie?>();
                foreach (var m in batch)
                    movies.Add(await FetchAndBuildMovieAsync(m.Id, ct));

                foreach (var movie in movies.OfType<Movie>())
                {
                    await db.Movies.AddAsync(movie, ct);
                    newMoviesCount++;
                }

                await db.SaveChangesAsync(ct);
            }

            if (page >= data.TotalPages)
                break;

            page++;
        }

        return Result.NoContent();
    }

    public async Task<Result> SyncRecentMoviesAsync(CancellationToken ct)
    {
        var today = DateTime.UtcNow;
        const int fromDays = 30;
        const string region = "US";

        var tmdbResponse = await tmdbApi.GetRecentReleasesAsync(
            today.ToString("yyyy-MM-dd"),
            today.AddDays(-fromDays).ToString("yyyy-MM-dd"),
            "release_date.desc",
            "3|2",
            region,
            false,
            30,
            1,
            ct);

        if (tmdbResponse.StatusCode == HttpStatusCode.Unauthorized)
            return Result.Unauthorized();

        if (!tmdbResponse.IsSuccessStatusCode || tmdbResponse.Content is null)
            return Result.Error("Failed to fetch recent releases from TMDB");

        foreach (var batch in tmdbResponse.Content.Results.Chunk(10))
        {
            var tmdbIds = batch.Select(m => m.Id).ToList();

            var existingIds = await db.Movies
                .Where(m => tmdbIds.Contains(m.ExternalId))
                .Select(m => m.ExternalId)
                .ToHashSetAsync(ct);

            var results = new List<(int tmdbId, Movie? movie, bool isExisting)>();
            foreach (var m in batch)
            {
                if (existingIds.Contains(m.Id))
                {
                    results.Add((m.Id, null, true));
                    continue;
                }
                var movie = await FetchAndBuildMovieAsync(m.Id, ct);
                results.Add((m.Id, movie, false));
            }

            var existingToUpdate = results
                .Where(r => r.isExisting)
                .Select(r => r.tmdbId)
                .ToList();

            if (existingToUpdate.Count > 0)
                await UpdateLocalizationsAsync(existingToUpdate, ct);

            foreach (var (_, movie, isExisting) in results)
            {
                if (!isExisting && movie is not null)
                    await db.Movies.AddAsync(movie, ct);
            }

            await db.SaveChangesAsync(ct);
        }

        return Result.NoContent();
    }

    public async Task<Result> SyncGenresAsync(CancellationToken ct)
    {
        var enTask = tmdbApi.GetGenresAsync(EnLang, ct);
        var ukTask = tmdbApi.GetGenresAsync(UaLang, ct);

        await Task.WhenAll(enTask, ukTask);

        var enResponse = await enTask;
        var ukResponse = await ukTask;

        if (!enResponse.IsSuccessStatusCode || !ukResponse.IsSuccessStatusCode)
            return Result.Error("Failed to fetch genres");

        var merged = enResponse.Content!.Genres.Join(
            ukResponse.Content!.Genres,
            en => en.Id,
            uk => uk.Id,
            (en, uk) => (en.Id, EnName: en.Name, UkName: uk.Name)
        );

        var existingGenres = await db.Genres
            .Include(g => g.Localizations)
            .ToListAsync(ct);

        foreach (var item in merged)
        {
            var existing = existingGenres.FirstOrDefault(g => g.ExternalId == item.Id);

            if (existing is null)
            {
                var genre = new Genre(item.Id);
                genre.AddLocalization(item.EnName, EnLang);
                genre.AddLocalization(item.UkName, UaLang);
                db.Genres.Add(genre);
            }
            else
            {
                existing.UpdateOrAddLocalization(item.EnName, EnLang);
                existing.UpdateOrAddLocalization(item.UkName, UaLang);
            }
        }

        await db.SaveChangesAsync(ct);
        return Result.Success();
    }

    private async Task<Movie?> FetchAndBuildMovieAsync(int tmdbId, CancellationToken ct)
    {
        var enTask = tmdbApi.GetMovieAsync(tmdbId, EnLang, ct);
        var ukTask = tmdbApi.GetMovieAsync(tmdbId, UaLang, ct);

        await Task.WhenAll(enTask, ukTask);

        var en = await enTask;
        var uk = await ukTask;

        if (!en.IsSuccessStatusCode || en.Content is null) return null;
        if (!uk.IsSuccessStatusCode || uk.Content is null) return null;

        return await ToMovieAsync(en.Content, uk.Content, ct);
    }

    private async Task UpdateLocalizationsAsync(List<int> tmdbIds, CancellationToken ct)
    {
        var tasks = tmdbIds.Select(async id =>
        {
            var enTask = tmdbApi.GetMovieAsync(id, EnLang, ct);
            var ukTask = tmdbApi.GetMovieAsync(id, UaLang, ct);
            await Task.WhenAll(enTask, ukTask);
            return (id, en: await enTask, uk: await ukTask);
        });

        var results = await Task.WhenAll(tasks);

        foreach (var (tmdbId, en, uk) in results)
        {
            if (!en.IsSuccessStatusCode || !uk.IsSuccessStatusCode) continue;

            var movie = await db.Movies
                .Include(m => m.Genres)
                .Include(m => m.Localizations)
                .FirstOrDefaultAsync(m => m.ExternalId == tmdbId, ct);

            if (movie is null) continue;

            foreach (var loc in movie.Localizations)
            {
                var source = loc.LanguageCode == EnLang ? en.Content : uk.Content;
                loc.Update(source!.Title, source.Overview);
            }

            if (en.Content!.Genres.Count > 0)
            {
                var genreIds = en.Content.Genres.Select(g => g.Id).ToList();
                var genres = await db.Genres
                    .Where(g => genreIds.Contains(g.ExternalId))
                    .ToListAsync(ct);

                if (genres.Count > 0)
                    movie.SyncGenres(genres);
            }
        }
    }

    private static ExternalMovie MapToDomain(TmdbMovieResponse tmdb)
    {
        var trailerKey = tmdb.Videos?.Results
            .FirstOrDefault(v => v is { Type: "Trailer", Site: "YouTube" })?.Key;

        var genres = new ExternalGenres(
            tmdb.Genres.Select(g => new ExternalGenreItem(g.Id, g.Name)).ToList()
        );

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
            trailerKey,
            genres
        );
    }

    private async Task<Movie> ToMovieAsync(TmdbMovieResponse en, TmdbMovieResponse uk, CancellationToken ct)
    {
        var trailerKey = en.Videos?.Results
            .FirstOrDefault(v => v is { Type: "Trailer", Site: "YouTube" })?.Key;

        var genreIds = en.Genres.Select(g => g.Id).ToList();

        var genres = await db.Genres
            .Where(g => genreIds.Contains(g.ExternalId))
            .ToListAsync(ct);

        var movie = new Movie(
            en.Id,
            en.ReleaseDate,
            en.PosterPath,
            en.VoteAverage,
            en.VoteCount,
            en.Popularity,
            en.BackdropPath,
            trailerKey
        );

        movie.AddGenres(genres);
        movie.AddLocalization(new MovieLocalization(EnLang, en.Title, en.Overview));
        movie.AddLocalization(new MovieLocalization(UaLang, uk.Title, uk.Overview));
        
        var slug = SlugExtensions.Slugify($"{en.Title}-{en.ReleaseDate.Year}");
        
        movie.AddSlug(slug);
        
        return movie;
    }
}