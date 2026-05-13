using JetBrains.Annotations;
using lumires.Core.Abstractions.Data;
using lumires.Core.Constants;
using Microsoft.EntityFrameworkCore;

namespace lumires.Api.Features.FilmsLists.GetFilmsList;

[UsedImplicitly]
internal class DataAccess(IAppDbContext db) : IDataAccess
{
    private const string DefLang = LocalizationConstants.DefaultCulture;

    public async Task<Response?> GetFilmsListAsync(Guid id, string lang, CancellationToken ct)
    {
        return await db.FilmsLists
            .AsNoTracking()
            .Where(c => c.Id == id)
            .Select(c => new Response(
                c.Id,
                c.Title,
                c.Description,
                c.User.Username,
                c.CreatedAt,
                c.Films
                    .OrderBy(m => m.Order)
                    .Select(m => new ListFilmItem(
                        m.Film.ExternalId,
                        m.Film.Localizations
                            .Where(l => l.LanguageCode == lang || l.LanguageCode == DefLang)
                            .OrderByDescending(l => l.LanguageCode == lang)
                            .Select(l => l.Title)
                            .SingleOrDefault() ?? string.Empty,
                        m.Film.PosterPath,
                        m.Order
                    ))
                    .ToList()
            ))
            .FirstOrDefaultAsync(ct);
    }
}