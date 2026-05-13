using FastEndpoints;
using JetBrains.Annotations;
using lumires.Core.Abstractions.Data;
using lumires.Core.Abstractions.Services;
using lumires.Core.Constants;
using lumires.Core.Events.Films;
using lumires.Core.Models;
using lumires.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Npgsql;

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
        if (command.ExternalIds.Count == 0) return;

        await using var scope = scopeFactory.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<IAppDbContext>();
        var personResolver = scope.ServiceProvider.GetRequiredService<IPersonResolver>();

        var distinctIds = command.ExternalIds.Distinct().ToList();

        var existingIds = await db.Films
            .Where(m => distinctIds.Contains(m.ExternalId))
            .Select(m => m.ExternalId)
            .ToListAsync(ct);

        var idsToFetch = distinctIds.Except(existingIds).ToList();

        if (idsToFetch.Count == 0) return;

        var semaphore = new SemaphoreSlim(Parallelism.MaxParallelism);
        var tasks = new List<Task<ExternalFilm?>>(idsToFetch.Count);

        tasks.AddRange(idsToFetch.Select(id => FetchFilm(id, command, semaphore, ct)));

        ExternalFilm?[] results;
        try
        {
            results = await Task.WhenAll(tasks);
        }
        finally
        {
            semaphore.Dispose();
        }

        var filmData = results.OfType<ExternalFilm>().ToList(); // to cut off nullable values

        if (filmData.Count == 0) return;

        var allGenreIds = filmData
            .SelectMany(m => m.Genres.Items)
            .Select(g => g.ExternalId)
            .Distinct()
            .ToList();

        var genres = await db.Genres
            .Where(g => allGenreIds.Contains(g.ExternalId))
            .ToListAsync(ct);

        var genreIdSet = genres.ToDictionary(g => g.ExternalId);

        var allPeople = filmData
            .SelectMany(m =>
                m.TopCast.Select(c => (c.Id, c.Name))
                    .Concat(m.Directors.Select(d => (d.Id, d.Name))))
            .Distinct()
            .ToList();

        var personDict = await personResolver.ResolveAsync(allPeople, ct);

        foreach (var data in filmData)
            try
            {
                var film = new Film(
                    Guid.CreateVersion7(),
                    data!.ExternalId,
                    data.ReleaseDate,
                    data.PosterPath,
                    data.VoteAverage,
                    data.VoteCount,
                    data.Popularity,
                    data.Runtime,
                    data.ProductionCompany,
                    data.BackdropPath,
                    data.TrailerUrl
                );

                var fimlGenres = data.Genres.Items
                    .Select(x => genreIdSet.GetValueOrDefault(x.ExternalId))
                    .OfType<Genre>();

                film.AddGenres(fimlGenres);

                film.AddSlug($"{data.Title}-{data.ReleaseDate.Year}");

                foreach (var c in data.TopCast.Where(c => personDict.ContainsKey(c.Id)))
                    film.AddCast(new FilmCast(
                        personDict[c.Id].Id,
                        c.Character,
                        c.Order));

                foreach (var d in data.Directors.Where(d => personDict.ContainsKey(d.Id)))
                    film.AddDirector(new FilmDirector(personDict[d.Id].Id));

                film.AddLocalization(new FilmLocalization(
                    command.Language,
                    data.Title,
                    data.Overview,
                    data.Tagline));

                db.Films.Add(film);
            }
            catch (Exception ex)
            {
                LogFilmProcessingFailed(logger, data.ExternalId, ex.Message);
            }

        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex)
        {
            var pgEx = ex.InnerException as PostgresException
                       ?? ex.InnerException?.InnerException as PostgresException;

            if (pgEx?.SqlState != "23505") throw;

            LogFilmAlreadyExistsBatch(logger);
        }
    }

    private async Task<ExternalFilm?> FetchFilm(
        int id,
        FilmReferencedEvent command,
        SemaphoreSlim semaphore,
        CancellationToken ct)
    {
        await semaphore.WaitAsync(ct);
        try
        {
            var result = await externalFilmService
                .GetFilmDetailsAsync(id, command.Language, ct);

            if (result.IsSuccess) return result.Value;
            LogFailedImport(logger, id);
            return null;
        }
        finally
        {
            semaphore.Release();
        }
    }

    [LoggerMessage(
        EventId = 2,
        Level = LogLevel.Warning,
        Message = "Failed to import movie {ExternalId}")]
    static partial void LogFailedImport(ILogger logger, int externalId);

    [LoggerMessage(
        EventId = 3,
        Level = LogLevel.Warning,
        Message = "Some movies already exist in batch")]
    static partial void LogFilmAlreadyExistsBatch(ILogger logger);

    [LoggerMessage(
        EventId = 4,
        Level = LogLevel.Warning,
        Message = "Failed processing movie {ExternalId}: {Error}")]
    static partial void LogFilmProcessingFailed(ILogger logger, int externalId, string error);
}