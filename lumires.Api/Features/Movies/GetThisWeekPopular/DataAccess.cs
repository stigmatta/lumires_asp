using JetBrains.Annotations;
using lumires.Core.Abstractions.Data;
using lumires.Core.Constants;
using Microsoft.EntityFrameworkCore;

namespace lumires.Api.Features.Movies.GetThisWeekPopular;

[UsedImplicitly]
internal class DataAccess(IAppDbContext db) : IDataAccess
{
    private const string DefLang = LocalizationConstants.DefaultCulture;

    internal async Task<Response?> GetThisWeekPopular(string lang, CancellationToken ct)
    {
        var items = await db.Movies
            .AsNoTracking()
            .OrderByDescending(movie => movie.Popularity)
            .Take(10)
            .Select(movie => new WeeklyPopularItem(
                movie.ExternalId,
                movie.Localizations
                    .Where(l => l.LanguageCode == lang || l.LanguageCode == DefLang)
                    .OrderByDescending(l => l.LanguageCode == lang)
                    .Select(l => l.Title)
                    .FirstOrDefault() ?? string.Empty,
                movie.VoteCount,
                movie.Slug,
                movie.TrailerUrl,
                movie.BackdropPath
            ))
            .ToListAsync(ct);

        return new Response(items);
    }
}