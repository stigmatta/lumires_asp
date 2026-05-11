using JetBrains.Annotations;
using lumires.Core.Abstractions.Data;
using lumires.Core.Constants;
using Microsoft.EntityFrameworkCore;

namespace lumires.Api.Features.Collections.GetCollection;

[UsedImplicitly]
internal class DataAccess(IAppDbContext db) : IDataAccess
{
    private const string DefLang = LocalizationConstants.DefaultCulture;

    public async Task<Response?> GetCollectionAsync(Guid id, string lang, CancellationToken ct)
    {
        return await db.Collections
            .AsNoTracking()
            .Where(c => c.Id == id)
            .Select(c => new Response(
                c.Id,
                c.Title,
                c.Description,
                c.User.Username,
                c.CreatedAt,
                c.Movies
                    .OrderBy(m => m.Order)
                    .Select(m => new CollectionMovieItem(
                        m.Movie.ExternalId,
                        m.Movie.Localizations
                            .Where(l => l.LanguageCode == lang || l.LanguageCode == DefLang)
                            .OrderByDescending(l => l.LanguageCode == lang)
                            .Select(l => l.Title)
                            .SingleOrDefault() ?? string.Empty,
                        m.Movie.PosterPath,
                        m.Order
                    ))
                    .ToList()
            ))
            .FirstOrDefaultAsync(ct);
    }
}