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
        var startOfWeek = DateTime.UtcNow.AddDays(-7);

        var raw = await db.Films
            .AsNoTracking()
            .Where(movie => movie.Reviews.Any(r => r.CreatedAt >= startOfWeek))
            .OrderByDescending(movie => movie.Reviews.Count(r => r.CreatedAt >= startOfWeek))
            .Take(6)
            .Select(m => new {
                m.ExternalId,
                m.Slug,
                m.BackdropPath,
                Title = m.Localizations
                    .Where(l => l.LanguageCode == lang || l.LanguageCode == DefLang)
                    .OrderByDescending(l => l.LanguageCode == lang)
                    .Select(l => l.Title)
                    .FirstOrDefault() ?? string.Empty,
                TopReview = m.Reviews
                    .Where(r => r.CreatedAt >= startOfWeek && r.Title != null)
                    .OrderByDescending(r => r.Likes.Count)
                    .Select(r => new { ReviewId = r.Id, r.Title, r.Rating, ReviewerId = r.Reviewer.Id , ReviewerName = r.Reviewer.Username})
                    .FirstOrDefault()
            })
            .ToListAsync(ct);

        var items = raw.Select(x => new WeeklyReviewedItem(
            x.ExternalId,
            x.TopReview?.ReviewId ?? Guid.Empty,
            x.Title,
            x.TopReview?.Title,
            x.Slug,
            x.BackdropPath,
            x.TopReview?.ReviewerId ?? Guid.Empty,
            x.TopReview?.ReviewerName ?? string.Empty,
            x.TopReview?.Rating
        )).ToList();

        return new Response(items);
    }
}