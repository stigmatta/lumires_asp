using JetBrains.Annotations;
using lumires.Core.Abstractions.Data;
using lumires.Core.Constants;
using Microsoft.EntityFrameworkCore;

namespace lumires.Api.Features.Films.GetThisWeekMostReviewed;

[UsedImplicitly]
internal class DataAccess(IAppDbContext db) : IDataAccess
{
    private const string DefLang = LocalizationConstants.DefaultCulture;

    internal async Task<Response?> GetThisWeekMostReviewed(string lang, CancellationToken ct)
    {
        var startOfWeek = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-7));

        var items = await db.Films
            .AsNoTracking()
            .Where(movie => movie.Reviews.Any(r => r.CreatedAt >= startOfWeek))
            .OrderByDescending(movie => movie.Reviews
                .Count(r => r.CreatedAt >= startOfWeek))
            .Take(6)
            .Select(m => new WeeklyReviewedItem(
                m.ExternalId,
                m.Localizations
                    .Where(l => l.LanguageCode == lang || l.LanguageCode == DefLang)
                    .OrderByDescending(l => l.LanguageCode == lang)
                    .Select(l => l.Title)
                    .FirstOrDefault() ?? string.Empty,
                m.Reviews
                    .Where(r => r.CreatedAt >= startOfWeek && r.Title != null)
                    .OrderByDescending(r => r.Likes.Count)
                    .Select(r => r.Title)
                    .FirstOrDefault(),
                m.Slug,
                m.BackdropPath,
                m.Reviews
                    .Where(r => r.CreatedAt >= startOfWeek && r.Title != null)
                    .OrderByDescending(r => r.Likes.Count)
                    .Select(r => r.Reviewer.Id)
                    .FirstOrDefault(),
                m.Reviews
                    .Where(r => r.CreatedAt >= startOfWeek && r.Title != null)
                    .OrderByDescending(r => r.Likes.Count)
                    .Select(r => r.Reviewer.Username)
                    .FirstOrDefault() ?? string.Empty,
                m.Reviews
                    .Where(r => r.CreatedAt >= startOfWeek && r.Title != null)
                    .OrderByDescending(r => r.Likes.Count)
                    .Select(r => r.Rating)
                    .FirstOrDefault()
            ))
            .ToListAsync(ct);

        return new Response(items);
    }
}