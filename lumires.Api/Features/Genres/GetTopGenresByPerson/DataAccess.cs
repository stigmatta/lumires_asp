// DataAccess.cs
using JetBrains.Annotations;
using lumires.Api.Features.Genres.Contracts;
using lumires.Core.Abstractions.Data;
using lumires.Core.Constants;
using Microsoft.EntityFrameworkCore;

namespace lumires.Api.Features.Genres.GetTopGenresByPerson;

[UsedImplicitly]
internal class DataAccess(IAppDbContext db) : IDataAccess
{
    private const string DefLang = LocalizationConstants.DefaultCulture;

    internal async Task<Response?> GetTopGenresByPersonAsync(int personId, string lang, CancellationToken ct)
    {
        var personExists = await db.Persons
            .AnyAsync(p => p.ExternalId == personId, ct);

        if (!personExists) return null;

        var genres = await db.Persons
            .AsNoTracking()
            .Where(p => p.ExternalId == personId)
            .SelectMany(p => p.FilmDirectors
                .SelectMany(fd => fd.Film.Genres)
                .Concat(p.FilmCasts
                    .SelectMany(fc => fc.Film.Genres)))
            .GroupBy(g => g.ExternalId)
            .OrderByDescending(g => g.Count())
            .Take(5)
            .Select(g => new GenreItem(
                g.Key,
                g.First().Localizations
                    .Where(l => l.LanguageCode == lang || l.LanguageCode == DefLang)
                    .OrderByDescending(l => l.LanguageCode == lang)
                    .Select(l => l.Name)
                    .FirstOrDefault() ?? string.Empty
            ))
            .ToListAsync(ct);

        return new Response(genres);
    }
}