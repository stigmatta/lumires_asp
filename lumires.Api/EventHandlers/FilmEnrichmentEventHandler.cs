using FastEndpoints;
using JetBrains.Annotations;
using lumires.Core.Abstractions.Data;
using lumires.Core.Abstractions.Services;
using lumires.Core.Constants;
using lumires.Core.Events.Films;
using lumires.Core.Models;
using lumires.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace lumires.Api.EventHandlers;

[UsedImplicitly]
internal sealed partial class FilmEnrichmentEventHandler(
    IServiceScopeFactory scopeFactory,
    IExternalFilmService externalFilmService,
    ILogger<FilmReferencedEventHandler> logger,
    IOptions<RequestLocalizationOptions> locOptions)
    : IEventHandler<FilmEnrichmentEvent>
{
    public async Task HandleAsync(FilmEnrichmentEvent command, CancellationToken ct)
    {
        if (command.ExternalIds.Count == 0) return;

        var cultures = locOptions.Value.SupportedCultures!
            .Select(c => c.Name)
            .Where(c => c != command.SkipLanguage)
            .ToList();

        if (cultures.Count == 0) return;

        try
        {
            var semaphore = new SemaphoreSlim(Parallelism.MaxParallelism);
            var tasks = new List<Task<(int ExternalId, string Culture, ExternalFilm Data)?>>();

            foreach (var id in command.ExternalIds)
                tasks.AddRange(cultures.Select(culture => FetchFilmLocalization(id, culture, semaphore, ct)));

            (int ExternalId, string Culture, ExternalFilm Data)?[] results;
            try
            {
                results = await Task.WhenAll(tasks);
            }
            finally
            {
                semaphore.Dispose();
            }

            var successful = results
                .OfType<(int ExternalId, string Culture, ExternalFilm Data)>()
                .ToList();

            if (successful.Count == 0) return;

            await using var scope = scopeFactory.CreateAsyncScope();
            var db = scope.ServiceProvider.GetRequiredService<IAppDbContext>();

            var externalIds = successful.Select(x => x.ExternalId).Distinct().ToList();

            var films = await db.Films
                .Include(m => m.Localizations)
                .Where(m => externalIds.Contains(m.ExternalId))
                .ToListAsync(ct);

            var filmDict = films.ToDictionary(m => m.ExternalId);

            var allPeople = successful
                .SelectMany(x => x.Data.TopCast.Select(c => (x.Culture, c.Id, c.Name))
                    .Concat(x.Data.Directors.Select(d => (x.Culture, d.Id, d.Name))))
                .ToList();

            var peopleExternalIds = allPeople.Select(p => p.Id).Distinct().ToList();

            var persons = await db.Persons
                .Include(p => p.Localizations)
                .Where(p => peopleExternalIds.Contains(p.ExternalId))
                .ToDictionaryAsync(p => p.ExternalId, ct);

            foreach (var (externalId, culture, data) in successful)
            {
                if (!filmDict.TryGetValue(externalId, out var film)) continue;

                if (film.Localizations.Any(l => l.LanguageCode == culture)) continue;

                film.AddLocalization(new FilmLocalization(
                    culture,
                    data.Title,
                    data.Overview,
                    data.Tagline));
            }

            foreach (var (culture, externalId, name) in allPeople)
            {
                if (!persons.TryGetValue(externalId, out var person)) continue;
                if (person.Localizations.Any(l => l.LanguageCode == culture)) continue;

                person.AddLocalization(new PersonLocalization(culture, name));
            }

            await db.SaveChangesAsync(ct);
        }
        catch (Exception ex)
        {
            LogUnexpectedError(logger, ex);
        }
    }

    private async Task<(int ExternalId, string Culture, ExternalFilm Data)?> FetchFilmLocalization(
        int id,
        string culture,
        SemaphoreSlim semaphore,
        CancellationToken ct)
    {
        await semaphore.WaitAsync(ct);
        try
        {
            var result = await externalFilmService.GetFilmDetailsAsync(id, culture, ct);
            if (result.IsSuccess) return (id, culture, result.Value);
            LogFailedImport(logger, id, culture);
            return null;
        }
        finally
        {
            semaphore.Release();
        }
    }

    [LoggerMessage(EventId = 2, Level = LogLevel.Warning,
        Message = "Failed to fetch film {ExternalId} for culture {Culture}")]
    static partial void LogFailedImport(ILogger logger, int externalId, string culture);

    [LoggerMessage(EventId = 5, Level = LogLevel.Error,
        Message = "Unexpected error during film enrichment")]
    static partial void LogUnexpectedError(ILogger logger, Exception exception);
}