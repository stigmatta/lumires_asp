using lumires.Core.Abstractions.Data;
using lumires.Core.Abstractions.Services;
using lumires.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace lumires.Api.Services;

internal class PersonResolver(IAppDbContext db) : IPersonResolver, IDataAccess
{
    public async Task<Dictionary<int, Person>> ResolveAsync(IEnumerable<(int ExternalId, string Name)> persons,
        CancellationToken ct)
    {
        var list = persons.DistinctBy(p => p.ExternalId).ToList();
        var externalIds = list.Select(p => p.ExternalId).ToList();

        var existing = await db.Persons
            .Where(p => externalIds.Contains(p.ExternalId))
            .ToListAsync(ct);

        var dict = existing.ToDictionary(p => p.ExternalId);

        foreach (var (externalId, name) in list)
        {
            if (dict.ContainsKey(externalId)) continue;

            var tracked = db.Persons.Local.FirstOrDefault(p => p.ExternalId == externalId);
            if (tracked is not null)
            {
                dict[externalId] = tracked;
                continue;
            }

            var person = new Person(externalId, name);
            await db.Persons.AddAsync(person, ct);
            dict[externalId] = person;
        }

        return dict;
    }
}