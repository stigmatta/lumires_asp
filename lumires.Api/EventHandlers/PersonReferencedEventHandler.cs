using FastEndpoints;
using JetBrains.Annotations;
using lumires.Core.Abstractions.Data;
using lumires.Core.Abstractions.Services;
using lumires.Core.Events.People;
using lumires.Core.Mappers;
using lumires.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace lumires.Api.EventHandlers;

[UsedImplicitly]
internal sealed partial class PersonReferencedEventHandler(
    IServiceScopeFactory scopeFactory,
    IExternalPersonService externalPersonService,
    ILogger<PersonReferencedEventHandler> logger)
    : IEventHandler<PersonReferencedEvent>
{
    public async Task HandleAsync(PersonReferencedEvent command, CancellationToken ct)
    {
        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<IAppDbContext>();

        foreach (var (externalId, expectedDepartment) in command.IdsAndDepartments)
        {
            var result = await externalPersonService
                .GetPersonDetailsAsync(externalId, command.Language, ct);

            if (!result.IsSuccess)
            {
                LogFailedImport(logger, externalId);
                continue;
            }

            var external = result.Value;

            if (!IsDepartmentMatch(external.KnownForDepartment, expectedDepartment))
            {
                LogSkip(logger, externalId, external.KnownForDepartment, expectedDepartment);
                continue;
            }

            var person = await db.Persons
                .Include(p => p.Localizations)
                .Include(p => p.Details)
                .FirstOrDefaultAsync(p => p.ExternalId == externalId, ct);

            var isNewPerson = person is null;

            if (isNewPerson)
            {
                person = new Person(externalId, PersonDepartmentMapper.FromString(expectedDepartment));
                db.Persons.Add(person);
            }

            if (person!.Localizations.All(l => l.LanguageCode != command.Language))
                person.AddLocalization(new PersonLocalization(command.Language, external.Name));

            if (person.Details.All(l => l.LanguageCode != command.Language))
            {
                var detail = new PersonDetail(
                    person.Id,
                    command.Language,
                    external.Biography,
                    external.Birthday,
                    external.Deathday,
                    external.Gender,
                    external.PlaceOfBirth,
                    external.ProfilePath);

                person.AddDetail(detail);
            }

            await db.SaveChangesAsync(ct);

            if (isNewPerson)
                await new PersonEnrichmentEvent
                {
                    IdsAndDepartments = [(externalId, expectedDepartment)],
                    SkipLanguage = command.Language
                }.PublishAsync(Mode.WaitForNone, ct);
        }
    }

    private static bool IsDepartmentMatch(string? knownForDepartment, string expectedDepartment)
    {
        return string.IsNullOrWhiteSpace(knownForDepartment) ||
               knownForDepartment.Equals(expectedDepartment, StringComparison.OrdinalIgnoreCase);
    }

    [LoggerMessage(EventId = 2, Level = LogLevel.Warning,
        Message = "Failed to import person {ExternalId}")]
    static partial void LogFailedImport(ILogger logger, int externalId);

    [LoggerMessage(EventId = 3, Level = LogLevel.Warning,
        Message =
            "Person {ExternalId} has KnownForDepartment '{KnownFor}', but requested as '{Requested}'. Skipping (404 logic)")]
    static partial void LogSkip(ILogger logger, int externalId, string? knownFor, string requested);
}