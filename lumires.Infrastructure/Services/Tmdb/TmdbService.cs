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

public sealed class TmdbService(
    ITmdbApi tmdbApi,
    IAppDbContext db,
    IPersonResolver personResolver,
    ILogger<TmdbService> logger) : IExternalFilmService
{
    private const string DefLang = LocalizationConstants.DefaultCulture;
    private const string EnLang = "en-US";
    private const string UaLang = "uk-UA";

    public async Task<Result<ExternalFilm>> GetFilmDetailsAsync(int movieId, string lang,
        CancellationToken ct = default)
    {
        var tmdbResponse = await tmdbApi.GetFilmAsync(movieId, lang, ct);

        switch (tmdbResponse.StatusCode)
        {
            case HttpStatusCode.Unauthorized:
                return Result.Unauthorized();
            case HttpStatusCode.NotFound:
                return Result.NotFound();
        }

        if (!tmdbResponse.IsSuccessStatusCode || tmdbResponse.Content == null) return Result.Error();

        var externalFilm = MapToDomain(tmdbResponse.Content);

        if ((!string.IsNullOrWhiteSpace(externalFilm.Overview) && externalFilm.TrailerUrl != null) || lang == DefLang)
            return externalFilm;

        var fallbackResponse = await tmdbApi.GetFilmShortenedAsync(movieId, DefLang, ct);
        if (fallbackResponse.Content == null) return externalFilm;

        var fallback = MapToDomain(fallbackResponse.Content);

        return externalFilm with
        {
            Overview = string.IsNullOrWhiteSpace(externalFilm.Overview) ? fallback.Overview : externalFilm.Overview,
            TrailerUrl = externalFilm.TrailerUrl ?? fallback.TrailerUrl
        };
    }

    public async Task<Result> SyncTrendingFilmsAsync(CancellationToken ct)
    {
        var trendingResponse = await tmdbApi.GetTrendingFilmsAsync(ct);

        if (trendingResponse.StatusCode == HttpStatusCode.Unauthorized)
            return Result.Unauthorized();

        if (!trendingResponse.IsSuccessStatusCode || trendingResponse.Content is null)
            return Result.Error("Failed to fetch trending films from TMDB");

        foreach (var batch in trendingResponse.Content.Results.Chunk(10))
        {
            var tmdbIds = batch.Select(m => m.Id).ToList();

            var existingIds = await db.Films
                .Where(m => tmdbIds.Contains(m.ExternalId))
                .Select(m => m.ExternalId)
                .ToHashSetAsync(ct);

            var results = new List<(int tmdbId, Film? film, bool isExisting)>();
            foreach (var m in batch)
            {
                if (existingIds.Contains(m.Id))
                {
                    results.Add((m.Id, null, true));
                    continue;
                }

                var movie = await FetchAndBuildFilmAsync(m.Id, ct);
                results.Add((m.Id, movie, false));
            }

            var existingToUpdate = results
                .Where(r => r.isExisting)
                .Select(r => r.tmdbId)
                .ToList();

            if (existingToUpdate.Count > 0)
                await UpdateLocalizationsAsync(existingToUpdate, ct);

            foreach (var (_, film, isExisting) in results)
                if (!isExisting && film is not null)
                    await db.Films.AddAsync(film, ct);

            await db.SaveChangesAsync(ct);
        }

        return Result.NoContent();
    }

    public async Task<Result> SyncPopularFilmsAsync(CancellationToken ct)
    {
        const int targetNewFilms = 40;
        var newFilmsCount = 0;
        var page = 1;
        const int maxPages = 10;

        while (newFilmsCount < targetNewFilms && page <= maxPages)
        {
            var popularResponse = await tmdbApi.GetPopularFilmsAsync(page, ct);

            if (!popularResponse.IsSuccessStatusCode || popularResponse.Content is null)
                break;

            var data = popularResponse.Content;
            var tmdbIds = data.Results.Select(m => m.Id).ToList();

            var existingIds = await db.Films
                .Where(m => tmdbIds.Contains(m.ExternalId))
                .Select(m => m.ExternalId)
                .ToHashSetAsync(ct);

            var newFilms = data.Results
                .Where(m => !existingIds.Contains(m.Id))
                .ToList();

            if (newFilms.Count == 0)
            {
                page++;
                continue;
            }

            foreach (var batch in newFilms.Chunk(10))
            {
                var films = new List<Film?>();
                foreach (var m in batch)
                    films.Add(await FetchAndBuildFilmAsync(m.Id, ct));

                foreach (var film in films.OfType<Film>())
                {
                    await db.Films.AddAsync(film, ct);
                    newFilmsCount++;
                }

                await db.SaveChangesAsync(ct);
            }

            if (page >= data.TotalPages)
                break;

            page++;
        }

        return Result.NoContent();
    }

    public async Task<Result> SyncRecentFilmsAsync(CancellationToken ct)
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

            var existingIds = await db.Films
                .Where(m => tmdbIds.Contains(m.ExternalId))
                .Select(m => m.ExternalId)
                .ToHashSetAsync(ct);

            var results = new List<(int tmdbId, Film? film, bool isExisting)>();
            foreach (var f in batch)
            {
                if (existingIds.Contains(f.Id))
                {
                    results.Add((f.Id, null, true));
                    continue;
                }

                var movie = await FetchAndBuildFilmAsync(f.Id, ct);
                results.Add((f.Id, movie, false));
            }

            var existingToUpdate = results
                .Where(r => r.isExisting)
                .Select(r => r.tmdbId)
                .ToList();

            if (existingToUpdate.Count > 0)
                await UpdateLocalizationsAsync(existingToUpdate, ct);

            foreach (var (_, film, isExisting) in results)
                if (!isExisting && film is not null)
                    await db.Films.AddAsync(film, ct);

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

    public async Task<Result> SyncCredits(int batchSize = 20, CancellationToken ct = default)
    {
        var filmsWithoutCredits = await db.Films
            .AsNoTracking()
            .Where(m => !m.Cast.Any() && !m.Directors.Any())
            .Select(m => new { m.Id, m.ExternalId })
            .Take(batchSize * 3)
            .ToListAsync(ct);

        logger.LogInformation("Found {Count} films without credits", filmsWithoutCredits.Count);

        if (filmsWithoutCredits.Count == 0)
            return Result.Success();

        foreach (var batch in filmsWithoutCredits.Chunk(batchSize))
        {
            foreach (var filmInfo in batch)
                try
                {
                    var response = await tmdbApi.GetFilmAsync(filmInfo.ExternalId, EnLang, ct);
                    if (response.Error != null)
                    {
                        logger.LogError(response.Error, "Refit error for tmdbId={TmdbId}: {Message}",
                            filmInfo.ExternalId, response.Error.Message);
                        continue;
                    }

                    switch (response.StatusCode)
                    {
                        case HttpStatusCode.Unauthorized:
                            return Result.Unauthorized();
                        case HttpStatusCode.NotFound:
                            return Result.NotFound();
                    }

                    if (!response.IsSuccessStatusCode || response.Content?.Credits == null)
                    {
                        logger.LogWarning("No credits in response for tmdbId={TmdbId}", filmInfo.ExternalId);
                        continue;
                    }

                    logger.LogInformation(
                        "Credits for tmdbId={TmdbId}: cast={CastCount}, crew={CrewCount}",
                        filmInfo.ExternalId,
                        response.Content.Credits.Cast?.Count ?? 0,
                        response.Content.Credits.Crew?.Count ?? 0);

                    await AddCreditsToExistingFilmAsync(filmInfo.ExternalId, response.Content.Credits, ct);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Failed to sync credits for tmdbId={TmdbId}", filmInfo.ExternalId);
                }

            var saved = await db.SaveChangesAsync(ct);
            logger.LogInformation("Saved {Count} changes", saved);
            await Task.Delay(300, ct);
        }

        return Result.Success();
    }

    public async Task<Result<long>> GetTotalFilmsCountAsync(CancellationToken ct)
    {
        var response = await tmdbApi.GetTotalFilmsCountAsync(ct: ct);
        if (!response.IsSuccessStatusCode || response.Content is null)
            return Result.Error("Failed to fetch total films from TMDB");
        
        long totalFilms = response.Content.TotalResults;

        return Result.Success(totalFilms);
    }

    public async Task<Result<IReadOnlyCollection<ExternalFilmShort>>> GetSimilarFilmsAsync(
        int movieId,
        string lang,
        CancellationToken ct)
    {
        var response = await tmdbApi.GetSimilarFilmsAsync(movieId, EnLang, ct);

        switch (response.StatusCode)
        {
            case HttpStatusCode.Unauthorized:
                return Result.Unauthorized();
            case HttpStatusCode.NotFound:
                return Result.NotFound();
        }

        if (!response.IsSuccessStatusCode || response.Content is null)
            return Result.Error("Failed to fetch similar films from TMDB");

        var items = response.Content.Results
            .Select(m => new ExternalFilmShort(
                m.Id,
                m.Title,
                m.PosterPath,
                m.ReleaseDate.Year,
                m.VoteAverage,
                m.VoteCount,
                m.Popularity
            ))
            .ToList();

        return Result.Success<IReadOnlyCollection<ExternalFilmShort>>(items);
    }

    private async Task<Film?> FetchAndBuildFilmAsync(int tmdbId, CancellationToken ct)
    {
        var enTask = tmdbApi.GetFilmAsync(tmdbId, EnLang, ct);
        var ukTask = tmdbApi.GetFilmShortenedAsync(tmdbId, UaLang, ct);

        await Task.WhenAll(enTask, ukTask);

        var en = await enTask;
        var uk = await ukTask;

        if (!en.IsSuccessStatusCode || en.Content is null) return null;
        if (!uk.IsSuccessStatusCode || uk.Content is null) return null;

        return await ToFilmAsync(en.Content, uk.Content, ct);
    }

    private async Task UpdateLocalizationsAsync(List<int> tmdbIds, CancellationToken ct)
    {
        var tasks = tmdbIds.Select(async id =>
        {
            var enTask = tmdbApi.GetFilmAsync(id, EnLang, ct);
            var ukTask = tmdbApi.GetFilmShortenedAsync(id, UaLang, ct);
            await Task.WhenAll(enTask, ukTask);
            return (id, en: await enTask, uk: await ukTask);
        });

        var results = await Task.WhenAll(tasks);

        foreach (var (tmdbId, en, uk) in results)
        {
            if (!en.IsSuccessStatusCode || !uk.IsSuccessStatusCode) continue;

            var movie = await db.Films
                .Include(m => m.Genres)
                .Include(m => m.Localizations)
                .FirstOrDefaultAsync(m => m.ExternalId == tmdbId, ct);

            if (movie is null) continue;

            foreach (var loc in movie.Localizations)
            {
                var source = loc.LanguageCode == EnLang ? en.Content : uk.Content;
                loc.Update(source!.Title, source.Overview, source.Tagline);
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

    private static ExternalFilm MapToDomain(TmdbMovieResponse tmdb)
    {
        var trailerKey = tmdb.Videos?.Results
            .FirstOrDefault(v => v is { Type: "Trailer", Site: "YouTube" })?.Key;

        var topCast = GetTopExternalCast(tmdb.Credits?.Cast);
        var directors = GetExternalDirectors(tmdb.Credits?.Crew);
        var company = GetProductionCompany(tmdb.ProductionCompanies);

        var genres = new ExternalGenres(
            tmdb.Genres.Select(g => new ExternalGenreItem(g.Id, g.Name)).ToList()
        );

        return new ExternalFilm(
            tmdb.Id,
            tmdb.Title,
            tmdb.Overview,
            tmdb.PosterPath,
            tmdb.VoteAverage,
            tmdb.VoteCount,
            tmdb.Popularity,
            tmdb.Runtime,
            company,
            tmdb.BackdropPath,
            tmdb.ReleaseDate,
            trailerKey,
            tmdb.Tagline,
            genres,
            topCast,
            directors
        );
    }

    private async Task<Film> ToFilmAsync(TmdbMovieResponse en, TmdbMovieResponse uk, CancellationToken ct)
    {
        var trailerKey = en.Videos?.Results
            .FirstOrDefault(v => v is { Type: "Trailer", Site: "YouTube" })?.Key;

        var genreIds = en.Genres.Select(g => g.Id).ToList();
        var topCastData = GetTopCastData(en.Credits?.Cast);
        var directorsData = GetDirectorsData(en.Credits?.Crew);
        var companyData = GetProductionCompany(en.ProductionCompanies);

        var genres = await db.Genres
            .Where(g => genreIds.Contains(g.ExternalId))
            .ToListAsync(ct);

        var personDict = await personResolver.ResolveAsync(
            topCastData.Select(c => (c.ExternalId, c.Name))
                .Concat(directorsData.Select(d => (d.ExternalId, d.Name))),
            EnLang,
            ct);

        var film = new Film(
            en.Id,
            en.ReleaseDate,
            en.PosterPath,
            en.VoteAverage,
            en.VoteCount,
            en.Popularity,
            en.Runtime,
            companyData,
            en.BackdropPath,
            trailerKey
        );

        film.AddGenres(genres);
        film.AddLocalization(new FilmLocalization(EnLang, en.Title, en.Overview, en.Tagline));
        film.AddLocalization(new FilmLocalization(UaLang, uk.Title, uk.Overview, uk.Tagline));
        film.AddSlug(SlugExtensions.Slugify($"{en.Title}-{en.ReleaseDate.Year}"));

        foreach (var c in topCastData)
            if (personDict.TryGetValue(c.ExternalId, out var person))
                film.AddCast(new FilmCast(person.Id, c.Character, c.Order));

        foreach (var d in directorsData)
            if (personDict.TryGetValue(d.ExternalId, out var person))
                film.AddDirector(new FilmDirector(person.Id));

        return film;
    }

    private static IReadOnlyCollection<ExternalCastMember> GetTopExternalCast(IReadOnlyList<CastMember>? cast)
    {
        if (cast is null || cast.Count == 0)
            return [];

        return cast
            .OrderBy(x => x.Order)
            .Take(6)
            .Select(x => new ExternalCastMember(
                x.Id,
                x.Name,
                x.Character ?? string.Empty,
                x.Order
            ))
            .ToList();
    }

    private static IReadOnlyCollection<ExternalDirector> GetExternalDirectors(IReadOnlyList<CrewMember>? crew)
    {
        if (crew is null || crew.Count == 0)
            return [];

        return crew
            .Where(x => x.Job == "Director")
            .Take(2)
            .Select(x => new ExternalDirector(x.Id, x.Name))
            .ToList();
    }

    private static string GetProductionCompany(IReadOnlyCollection<TmdbProductionCompanyItem> companies)
    {
        if (companies.Count == 0)
            return string.Empty;

        return companies
            .Select(c => c.Name)
            .FirstOrDefault() ?? string.Empty;
    }

    private static List<(int ExternalId, string Name, string Character, int Order)>
        GetTopCastData(IReadOnlyList<CastMember>? cast)
    {
        if (cast is null || cast.Count == 0) return [];

        return cast
            .OrderBy(x => x.Order)
            .Take(6)
            .Select(x => (x.Id, x.Name, x.Character ?? "", x.Order))
            .ToList();
    }

    private static List<(int ExternalId, string Name)>
        GetDirectorsData(IReadOnlyList<CrewMember>? crew)
    {
        if (crew is null || crew.Count == 0) return [];

        return crew
            .Where(x => x.Job == "Director")
            .Take(2)
            .Select(x => (x.Id, x.Name))
            .ToList();
    }

    private async Task AddCreditsToExistingFilmAsync(int movieId, CreditsResponse credits, CancellationToken ct)
    {
        var movie = await db.Films
            .Include(m => m.Cast)
            .Include(m => m.Directors)
            .FirstOrDefaultAsync(m => m.ExternalId == movieId, ct);

        if (movie is null || movie.Cast.Count > 0 || movie.Directors.Count > 0)
            return;

        var topCastData = GetTopCastData(credits.Cast);
        var directorsData = GetDirectorsData(credits.Crew);

        var personDict = await personResolver.ResolveAsync(
            topCastData.Select(c => (c.ExternalId, c.Name))
                .Concat(directorsData.Select(d => (d.ExternalId, d.Name))),
            EnLang,
            ct);

        foreach (var c in topCastData)
            if (personDict.TryGetValue(c.ExternalId, out var person))
                movie.AddCast(new FilmCast(person.Id, c.Character, c.Order));

        foreach (var d in directorsData)
            if (personDict.TryGetValue(d.ExternalId, out var person))
                movie.AddDirector(new FilmDirector(person.Id));
    }
}