using JetBrains.Annotations;
using lumires.Api.Features.Genres.Contracts;
using lumires.Core.Abstractions.Data;
using lumires.Core.Constants;
using Microsoft.EntityFrameworkCore;

namespace lumires.Api.Features.Genres.GetGenres;

[UsedImplicitly]
internal class DataAccess(IAppDbContext db) : IDataAccess
{
    private const string DefLang = LocalizationConstants.DefaultCulture;

    internal async Task<Response> GetGenres(string lang, CancellationToken ct)
    {
        var genres = await db.Genres
            .AsNoTracking()
            .Select(g => new GenreItem(
                g.ExternalId,
                g.Localizations
                    .Where(l => l.LanguageCode == lang || l.LanguageCode == DefLang)
                    .OrderByDescending(l => l.LanguageCode == lang)
                    .Select(l => l.Name)
                    .FirstOrDefault() ?? string.Empty
            ))
            .ToListAsync(ct);

        return new Response(genres);
    }
}