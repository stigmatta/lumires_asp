using FastEndpoints;
using JetBrains.Annotations;
using lumires.Core.Abstractions.Data;
using lumires.Core.Abstractions.Services;
using lumires.Core.Constants;
using lumires.Core.Events.People;
using lumires.Core.Models;
using lumires.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace lumires.Api.EventHandlers;

[UsedImplicitly]
internal sealed partial class PersonEnrichmentEventHandler(
    IServiceScopeFactory scopeFactory,
    IExternalPersonService externalPersonService,
    ILogger<PersonEnrichmentEventHandler> logger,
    IOptions<RequestLocalizationOptions> locOptions)
    : IEventHandler<PersonEnrichmentEvent>
{
    public async Task HandleAsync(PersonEnrichmentEvent command, CancellationToken ct)
    {
        if (command.IdsAndDepartments.Count == 0)
            return;

        var cultures = locOptions.Value.SupportedCultures!
            .Select(c => c.Name)
            .Where(c => c != command.SkipLanguage)
            .ToList();

        if (cultures.Count == 0)
            return;

        try
        {
            using var semaphore = new SemaphoreSlim(Parallelism.MaxParallelism);

            var tasks = command.IdsAndDepartments
                .Select(item => FetchPersonCultures(item.Id, cultures, semaphore, ct))
                .ToList();

            var results = await Task.WhenAll(tasks);

            var successful = results
                .Where(x => x is not null)
                .SelectMany(x => x!)
                .ToList();

            if (successful.Count == 0)
                return;

            await using var scope = scopeFactory.CreateAsyncScope();
            var db = scope.ServiceProvider.GetRequiredService<IAppDbContext>();

            var personIds = successful
                .Select(x => x.ExternalId)
                .Distinct()
                .ToList();

            var persons = await db.Persons
                .Include(p => p.Localizations)
                .Include(p => p.Details)
                .Where(p => personIds.Contains(p.ExternalId))
                .ToDictionaryAsync(p => p.ExternalId, ct);

            foreach (var item in successful)
            {
                if (!persons.TryGetValue(item.ExternalId, out var person))
                    continue;

                var culture = item.Culture;
                var data = item.Data;

                if (person.Localizations.All(l => l.LanguageCode != culture))
                    person.AddLocalization(new PersonLocalization(culture, data.Name));

                if (person.Details.Any(d => d.LanguageCode == culture)) continue;

                var detail = new PersonDetail(
                    person.Id,
                    culture,
                    data.Biography,
                    data.Birthday,
                    data.Deathday,
                    data.Gender,
                    data.PlaceOfBirth,
                    data.ProfilePath);

                person.AddDetail(detail);
            }

            await db.SaveChangesAsync(ct);
        }
        catch (Exception ex)
        {
            LogUnexpectedError(logger, ex);
        }
    }

    private async Task<List<(int ExternalId, string Culture, ExternalPerson Data)>?> FetchPersonCultures(
        int externalId,
        List<string> cultures,
        SemaphoreSlim semaphore,
        CancellationToken ct)
    {
        await semaphore.WaitAsync(ct);

        try
        {
            var tasks = cultures.Select(async culture =>
            {
                var result = await externalPersonService.GetPersonDetailsAsync(externalId, culture, ct);

                return result.IsSuccess
                    ? (ExternalId: externalId, Culture: culture, Data: result.Value)
                    : default;
            });

            var results = await Task.WhenAll(tasks);

            var filtered = results
                .Where(x => x != default)
                .ToList();

            return filtered.Count == 0 ? null : filtered;
        }
        catch (Exception)
        {
            LogFailedImport(logger, externalId);
            return null;
        }
        finally
        {
            semaphore.Release();
        }
    }

    [LoggerMessage(EventId = 1, Level = LogLevel.Warning,
        Message = "Failed to fetch person {ExternalId}")]
    static partial void LogFailedImport(ILogger logger, int externalId);

    [LoggerMessage(EventId = 5, Level = LogLevel.Error,
        Message = "Unexpected error during person enrichment")]
    static partial void LogUnexpectedError(ILogger logger, Exception exception);
}