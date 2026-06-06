using JetBrains.Annotations;
using lumires.Core.Abstractions.Data;
using lumires.Core.Constants;
using Microsoft.EntityFrameworkCore;

namespace lumires.Api.Features.Reviews.GetThisWeekTrendingReviews;

[UsedImplicitly]
internal class DataAccess(IAppDbContext db) : IDataAccess
{
    private const string DefLang = LocalizationConstants.DefaultCulture;

    internal async Task<Response?> GetTrendingReviewsWeeklyAsync(string lang, CancellationToken ct)
    {
        var weekAgo = DateTime.UtcNow.AddDays(-7);

        var items = await db.Reviews
            .AsNoTracking()
            .OrderByDescending(x =>
                x.Likes.Count(l => l.LikedAt >= weekAgo) +
                x.ReviewComments.Count(c => c.CreatedAt >= weekAgo) * 2)
            .Take(6)
            .Select(x => new TrendingReviewItem(
                x.Id,
                x.Film.ExternalId,
                x.Film.Localizations
                    .Where(l => l.LanguageCode == lang || l.LanguageCode == DefLang)
                    .OrderByDescending(l => l.LanguageCode == lang)
                    .Select(l => l.Title)
                    .FirstOrDefault() ?? string.Empty,
                x.Film.Slug,
                x.Title!,
                x.Rating,
                x.UserId,
                x.Reviewer.Username
            ))
            .ToListAsync(ct);

        return new Response(items);
    }
}