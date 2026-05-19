using JetBrains.Annotations;
using lumires.Core.Abstractions.Data;
using lumires.Core.Constants;
using lumires.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace lumires.Api.Features.Films.GetFilm;

[UsedImplicitly]
internal class DataAccess(IAppDbContext db) : IDataAccess
{
    private const string DefLang = LocalizationConstants.DefaultCulture;

    internal async Task<Response?> GetFilmByIdAsync(int tmdbId, string lang, CancellationToken ct)
    {
        return await db.Films
            .AsNoTracking()
            .Where(m => m.ExternalId == tmdbId)
            .Select(m => new Response(
                m.ExternalId,
                m.ReleaseDate,
                m.TrailerUrl,
                m.PosterPath,
                m.BackdropPath,
                m.Localizations
                    .Where(l => l.LanguageCode == lang || l.LanguageCode == DefLang)
                    .OrderByDescending(l => l.LanguageCode == lang)
                    .Select(l => new LocalizationResponse(
                        l.LanguageCode,
                        l.Title,
                        l.Description,
                        l.Tagline
                    ))
                    .FirstOrDefault(),
                new GenresResponse(m.Genres
                    .Select(g => new GenreItemResponse(
                        g.ExternalId,
                        g.Localizations
                            .Where(gl => gl.LanguageCode == lang || gl.LanguageCode == DefLang)
                            .OrderByDescending(gl => gl.LanguageCode == lang)
                            .Select(gl => gl.Name)
                            .FirstOrDefault() ?? string.Empty,
                        lang
                    ))
                    .ToList()),
                m.Cast
                    .Select(c => new PersonShortItem(
                        GetPersonNameWithFallback(c.Person.Localizations, lang),
                        c.Person.ExternalId
                    ))
                    .ToList(),
                m.Directors
                    .Select(c => new PersonShortItem(
                        GetPersonNameWithFallback(c.Person.Localizations, lang),
                        c.Person.ExternalId
                    ))
                    .ToList(),
                m.ProductionCompany,
                m.Runtime,
                m.VoteAverage,
                m.VoteCount
            ))
            .SingleOrDefaultAsync(ct);
    }

    private static string GetPersonNameWithFallback(
        IReadOnlyCollection<PersonLocalization> localizations, 
        string requestedLang)
    {
        if (localizations.Count == 0)
            return string.Empty;

        var exact = localizations.FirstOrDefault(l => l.LanguageCode == requestedLang);
        if (exact != null)
            return exact.Name;

        var fallback = localizations.FirstOrDefault(l => l.LanguageCode == DefLang);
        return fallback != null ? fallback.Name : localizations.First().Name;
    }
}