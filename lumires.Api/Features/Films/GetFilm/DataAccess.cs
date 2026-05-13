using JetBrains.Annotations;
using lumires.Core.Abstractions.Data;
using lumires.Core.Constants;
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
                            .Where(l => l.LanguageCode == lang || l.LanguageCode == DefLang)
                            .OrderByDescending(l => l.LanguageCode == lang)
                            .Select(l => l.Name)
                            .FirstOrDefault() ?? string.Empty,
                        lang
                    ))
                    .ToList()),
                m.Cast
                    .OrderByDescending(c => c.Order)
                    .Select(c => c.Person.Name)
                    .ToList(),
                m.Directors
                    .Select(d => d.Person.Name)
                    .ToList(),
                m.ProductionCompany,
                m.Runtime
            ))
            .SingleOrDefaultAsync(ct);
    }
}