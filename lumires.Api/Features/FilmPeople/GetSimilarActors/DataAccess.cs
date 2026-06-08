using JetBrains.Annotations;
using lumires.Core.Abstractions.Data;
using lumires.Core.Constants;
using lumires.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace lumires.Api.Features.FilmPeople.GetSimilarActors;

[UsedImplicitly]
internal class DataAccess(IAppDbContext db) : IDataAccess
{
    private const string DefLang = LocalizationConstants.DefaultCulture;

    internal async Task<Response> GetSimilarActors(int actorId, string lang, CancellationToken ct)
    {
        var actorGenres = await db.FilmCasts
            .Where(fd => fd.Person.ExternalId == actorId)
            .SelectMany(fd => fd.Film.Genres.Select(g => g.ExternalId))
            .Distinct()
            .ToListAsync(ct);

        if (actorGenres.Count == 0) return new Response([]);

        var similar = await db.Persons
            .AsNoTracking()
            .Where(p => p.PersonDepartment == PersonDepartment.Acting
                        && p.ExternalId != actorId)
            .Select(p => new
            {
                p.ExternalId,
                Name = p.Localizations
                    .Where(l => l.LanguageCode == lang || l.LanguageCode == DefLang)
                    .OrderByDescending(l => l.LanguageCode == lang)
                    .Select(l => l.Name)
                    .FirstOrDefault() ?? "Unknown",
                ProfilePath = p.Details
                    .Where(d => d.LanguageCode == lang || d.LanguageCode == DefLang)
                    .OrderByDescending(d => d.LanguageCode == lang)
                    .Select(d => d.ProfilePath)
                    .FirstOrDefault(),
                Genres = p.FilmDirectors
                    .SelectMany(fd => fd.Film.Genres.Select(g => g.ExternalId))
                    .Distinct()
                    .ToList()
            })
            .ToListAsync(ct);

        var items = similar
            .Select(p => new
            {
                Person = p,
                Intersection = p.Genres.Intersect(actorGenres).Count(),
                Union = p.Genres.Union(actorGenres).Count()
            })
            .Where(x => x.Union > 0 && x.Intersection > 0)
            .Select(x => new ActorItem(
                x.Person.ExternalId,
                x.Person.ProfilePath,
                x.Person.Name
            ))
            .Take(4)
            .ToList();

        return new Response(items);
    }
}