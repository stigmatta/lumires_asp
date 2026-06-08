using FastEndpoints;
using lumires.Core.Abstractions.Data;
using lumires.Core.Abstractions.Services;
using lumires.Core.Events.People;
using lumires.Core.Mappers;
using lumires.Domain.Entities;
using lumires.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace lumires.Api.Services;

public sealed partial class PersonResolver(IAppDbContext db, ILogger<PersonResolver> logger)
    : IPersonResolver, IResolver
{
    public async Task<Dictionary<int, Person>> ResolveAsync(
        IEnumerable<(int ExternalId, string Name, string department)> persons,
        string languageCode,
        CancellationToken ct = default)
    {
        var personData = persons.ToList();
        if (personData.Count == 0)
            return [];

        var externalIds = personData.Select(p => p.ExternalId).Distinct().ToList();

        var existingPersons = await db.Persons
            .Include(p => p.Localizations)
            .Where(p => externalIds.Contains(p.ExternalId))
            .ToDictionaryAsync(p => p.ExternalId, ct);

        var result = new Dictionary<int, Person>();
        var personsToAdd = new List<Person>();

        foreach (var (externalId, name, department) in personData)
        {
            if (existingPersons.TryGetValue(externalId, out var existing))
            {
                if (existing.Localizations.All(l => l.LanguageCode != languageCode))
                    existing.AddLocalization(new PersonLocalization(languageCode, name));
                result[externalId] = existing;
                continue;
            }

            if (result.ContainsKey(externalId))
                continue;

            var tracked = db.Persons.Local.FirstOrDefault(p => p.ExternalId == externalId);
            if (tracked is not null)
            {
                if (tracked.Localizations.All(l => l.LanguageCode != languageCode))
                    tracked.AddLocalization(new PersonLocalization(languageCode, name));
                result[externalId] = tracked;
                continue;
            }

            var newPerson = new Person(externalId, PersonDepartmentMapper.FromString(department));
            newPerson.AddLocalization(new PersonLocalization(languageCode, name));
            personsToAdd.Add(newPerson);
            result[externalId] = newPerson;
        }

        if (personsToAdd.Count <= 0) return result;

        await db.Persons.AddRangeAsync(personsToAdd, ct);
        LogPersonsAdded(logger, personsToAdd.Count);

        return result;
    }

    public async Task<bool> EnsurePersonExistsAsync(
        (int externalId, string Deparment) idAndDep,
        string language,
        CancellationToken ct)
    {
        var person = await db.Persons
            .Include(p => p.Details)
            .FirstOrDefaultAsync(m => m.ExternalId == idAndDep.externalId
                                      && m.PersonDepartment == PersonDepartment.Directing, ct);

        if (person is not null && person.Details.Any(d => d.LanguageCode == language) && person.Localizations.Any(d => d.LanguageCode == language))
            return true;

        await new PersonReferencedEvent
            {
                IdsAndDepartments = [idAndDep],
                Language = language
            }
            .PublishAsync(Mode.WaitForAll, ct);

        return false;
    }

    [LoggerMessage(
        EventId = 1,
        Level = LogLevel.Information,
        Message = "Added {Count} new persons to database")]
    private static partial void LogPersonsAdded(ILogger logger, int count);
}