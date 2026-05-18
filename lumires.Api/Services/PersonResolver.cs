using lumires.Core.Abstractions.Data;
using lumires.Core.Abstractions.Services;
using lumires.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace lumires.Api.Services;

public sealed partial class PersonResolver(IAppDbContext db, ILogger<PersonResolver> logger) : IPersonResolver, IResolver
{
    public async Task<Dictionary<int, Person>> ResolveAsync(
        IEnumerable<(int ExternalId, string Name)> persons,
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

        foreach (var (externalId, name) in personData)
            if (existingPersons.TryGetValue(externalId, out var existing))
            {
                if (existing.Localizations.All(l => l.LanguageCode != languageCode))
                    existing.AddLocalization(new PersonLocalization(languageCode, name));
                result[externalId] = existing;
            }
            else if (!result.ContainsKey(externalId))
            {
                var newPerson = new Person(externalId);
                newPerson.AddLocalization(new PersonLocalization(languageCode, name));

                personsToAdd.Add(newPerson);
                result[externalId] = newPerson;
            }

        if (personsToAdd.Count <= 0) return result;

        await db.Persons.AddRangeAsync(personsToAdd, ct);
        LogPersonsAdded(logger, personsToAdd.Count);

        return result;
    }

    [LoggerMessage(
        EventId = 1,
        Level = LogLevel.Information,
        Message = "Added {Count} new persons to database")]
    private static partial void LogPersonsAdded(ILogger logger, int count);
}