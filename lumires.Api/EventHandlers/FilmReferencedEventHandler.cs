using FastEndpoints;
using JetBrains.Annotations;
using lumires.Core;
using lumires.Core.Abstractions.Data;
using lumires.Core.Abstractions.Services;
using lumires.Core.Constants;
using lumires.Core.Events.Films;
using lumires.Core.Mappers;
using lumires.Core.Models;
using lumires.Domain.Entities;
using lumires.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace lumires.Api.EventHandlers;

[UsedImplicitly]
internal sealed partial class FilmReferencedEventHandler(
    IServiceScopeFactory scopeFactory,
    IExternalFilmService externalFilmService,
    ILogger<FilmReferencedEventHandler> logger)
    : IEventHandler<FilmReferencedEvent>
{
    public async Task HandleAsync(FilmReferencedEvent command, CancellationToken ct)
    {
        if (command.ExternalIds.Count == 0)
            return;

        await using var scope = scopeFactory.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<IAppDbContext>();
        var personResolver = scope.ServiceProvider.GetRequiredService<IPersonResolver>();

        var distinctIds = command.ExternalIds.Distinct().ToList();

        var existingIds = await db.Films
            .Where(m => distinctIds.Contains(m.ExternalId))
            .Select(m => m.ExternalId)
            .ToListAsync(ct);

        var idsToFetch = distinctIds.Except(existingIds).ToList();

        if (idsToFetch.Count == 0)
            return;

        var filmsData = await FetchFilmsBatch(idsToFetch, command.Language, ct);

        if (filmsData.Count == 0)
            return;

        var allGenreIds = filmsData
            .SelectMany(f => f.Genres.Items)
            .Select(g => g.ExternalId)
            .Distinct()
            .ToList();

        var genres = await db.Genres
            .Where(g => allGenreIds.Contains(g.ExternalId))
            .ToListAsync(ct);

        var genreDict = genres.ToDictionary(g => g.ExternalId);

        var allPeople = filmsData
            .SelectMany(f =>
                f.TopCast.Select(c => (c.Id, c.Name, Department: PersonDepartment.Acting))
                    .Concat(f.Directors.Select(d => (d.Id, d.Name, Department: PersonDepartment.Directing))))
            .Distinct()
            .ToList();

        var personDict = await personResolver.ResolveAsync(
            allPeople.Select(p => (p.Id, p.Name, PersonDepartmentMapper.ToString(p.Department))),
            command.Language,
            ct);

        foreach (var data in filmsData)
            try
            {
                var film = new Film(
                    data.ExternalId,
                    data.ReleaseDate,
                    data.PosterPath,
                    (float)Math.Round(data.VoteAverage / 2.0, 1),
                    data.VoteCount,
                    data.Popularity,
                    data.Runtime,
                    data.ProductionCompany,
                    data.BackdropPath,
                    data.TrailerUrl);

                var filmGenres = data.Genres.Items
                    .Select(x => genreDict.GetValueOrDefault(x.ExternalId))
                    .OfType<Genre>();

                film.AddGenres(filmGenres);

                var slugifiedTitle = SlugExtensions.Slugify($"{data.Title}-{data.ReleaseDate.Year}");
                film.AddSlug(slugifiedTitle);

                foreach (var c in data.TopCast.Where(c => personDict.ContainsKey(c.Id)))
                    film.AddCast(new FilmCast(personDict[c.Id].Id, c.Character, c.Order));

                foreach (var d in data.Directors.Where(d => personDict.ContainsKey(d.Id)))
                    film.AddDirector(new FilmDirector(personDict[d.Id].Id));

                film.AddLocalization(new FilmLocalization(
                    command.Language,
                    data.Title,
                    data.Overview,
                    data.Tagline));

                db.Films.Add(film);

                await db.SaveChangesAsync(ct);


                await new FilmEnrichmentEvent
                {
                    ExternalIds = [data.ExternalId],
                    SkipLanguage = command.Language
                }.PublishAsync(Mode.WaitForNone, ct);
            }
            catch (Exception ex)
            {
                LogFilmProcessingFailed(logger, data.ExternalId, ex.Message);
            }
    }

    private async Task<List<ExternalFilm>> FetchFilmsBatch(List<int> ids, string language, CancellationToken ct)
    {
        using var semaphore = new SemaphoreSlim(Parallelism.MaxParallelism);
        var tasks = ids.Select(id => FetchSingleFilm(id, language, semaphore, ct))
            .ToList();

        try
        {
            var results = await Task.WhenAll(tasks);
            return [.. results.OfType<ExternalFilm>()];
        }
        catch (Exception ex)
        {
            LogUnexpectedError(logger, ex);
            throw;
        }
    }

    private async Task<ExternalFilm?> FetchSingleFilm(int id, string language, SemaphoreSlim semaphore,
        CancellationToken ct)
    {
        await semaphore.WaitAsync(ct);
        try
        {
            var result = await externalFilmService.GetFilmDetailsAsync(id, language, ct);
            return result.IsSuccess ? result.Value : null;
        }
        catch (Exception)
        {
            LogFailedImport(logger, id);
            return null;
        }
        finally
        {
            semaphore.Release();
        }
    }

    [LoggerMessage(EventId = 2, Level = LogLevel.Warning, Message = "Failed to import movie {ExternalId}")]
    static partial void LogFailedImport(ILogger logger, int externalId);

    [LoggerMessage(EventId = 4, Level = LogLevel.Warning, Message = "Failed processing movie {ExternalId}: {Error}")]
    static partial void LogFilmProcessingFailed(ILogger logger, int externalId, string error);

    [LoggerMessage(
        EventId = 3,
        Level = LogLevel.Error,
        Message = "Error while fetching films batch.")]
    static partial void LogUnexpectedError(ILogger logger, Exception error);
}