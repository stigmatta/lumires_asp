using Core.Abstractions.Data;
using Core.Constants;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;

namespace Api.Features.Movies.GetMovie;

[UsedImplicitly]
internal sealed class Queries(IAppDbContext db) : IQuery
{
    private const string DefLang = LocalizationConstants.DefaultCulture;

    internal async Task<Response?> GetMovieByIdAsync(int tmdbId, string lang, CancellationToken ct)
    {
        return await db.Movies
            .AsNoTracking()
            .Where(m => m.ExternalId == tmdbId)
            .Select(m => new Response(
                m.ExternalId,
                m.Year,
                m.Localizations
                    .Where(l => l.LanguageCode == lang || l.LanguageCode == DefLang)
                    .OrderByDescending(l => l.LanguageCode == lang) 
                    .Select(l => new LocalizationResponse(
                        l.LanguageCode,
                        l.Title,
                        l.Description
                    ))
                    .FirstOrDefault() 
            ))
            .SingleOrDefaultAsync(ct);
    }
}