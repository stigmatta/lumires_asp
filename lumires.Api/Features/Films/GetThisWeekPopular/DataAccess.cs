using JetBrains.Annotations;
using lumires.Core.Abstractions.Data;
using lumires.Core.Constants;
using Microsoft.EntityFrameworkCore;

namespace lumires.Api.Features.Films.GetThisWeekPopular;

[UsedImplicitly]
internal class DataAccess(IAppDbContext db) : IDataAccess
{
    private const string DefLang = LocalizationConstants.DefaultCulture;

    internal async Task<Response?> GetThisWeekPopular(string lang, CancellationToken ct)
    {
        var startOfWeek = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-7));
        
        var items = await db.Films
            .AsNoTracking()
            .OrderByDescending(movie => movie.Reviews
                .Count(r => r.CreatedAt >= startOfWeek))
            .ThenByDescending(movie => movie.Popularity)
            .Take(10)
            .Select(movie => new WeeklyPopularItem(
                movie.ExternalId,
                movie.Localizations
                    .Where(l => l.LanguageCode == lang || l.LanguageCode == DefLang)
                    .OrderByDescending(l => l.LanguageCode == lang)
                    .Select(l => l.Title)
                    .FirstOrDefault() ?? string.Empty,
                movie.ReleaseDate.HasValue ? movie.ReleaseDate.Value.Year : null,
                movie.VoteCount + movie.UserRatings.Count,
                movie.Slug,
                movie.TrailerUrl,
                movie.BackdropPath
            ))
            .ToListAsync(ct);

        return new Response(items);
    }
}