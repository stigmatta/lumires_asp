using JetBrains.Annotations;
using lumires.Core.Abstractions.Data;
using lumires.Core.Constants;
using Microsoft.EntityFrameworkCore;

namespace lumires.Api.Features.Films.GetThisWeekRecentReleases;

[UsedImplicitly]
internal class DataAccess(IAppDbContext db) : IDataAccess
{
    private const string DefLang = LocalizationConstants.DefaultCulture;

    internal async Task<Response?> GetThisWeekRecentReleases(string lang, CancellationToken ct)
    {
        var items = await db.Films
            .AsNoTracking()
            .Where(movie => movie.ReleaseDate >= DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-30)))
            .OrderByDescending(movie => movie.ReleaseDate)
            .Take(10)
            .Select(movie => new WeeklyRecentItem(
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