using JetBrains.Annotations;
using lumires.Api.Extensions;
using lumires.Core.Abstractions.Data;
using lumires.Core.Constants;
using Microsoft.EntityFrameworkCore;

namespace lumires.Api.Features.Reviews.GetRecentReviews;

[UsedImplicitly]
internal class DataAccess(IAppDbContext db) : IDataAccess
{
    private const string DefLang = LocalizationConstants.DefaultCulture;

    internal async Task<List<RecentReviewItem>> GetRecentReviewsAsync(Query query, string lang, Guid userId,
        CancellationToken ct)
    {
        return await db.Reviews
            .OrderByDescending(r => r.CreatedAt)
            .ApplyPaging(query.Page, query.PageSize)
            .Select(r => new RecentReviewItem(
                r.Id,
                r.UserId,
                r.Reviewer.Username,
                r.Reviewer.AvatarUrl,
                r.ReviewComments.Count,
                r.Rating,
                r.Title,
                r.Text,
                r.LikesCount,
                r.CreatedAt,
                userId != Guid.Empty && r.Likes.Any(l => l.UserId == userId),
                r.IsSpoilerFree,
                r.Film.ExternalId,
                r.Film.Localizations
                    .Where(l => l.LanguageCode == lang || l.LanguageCode == DefLang)
                    .OrderByDescending(l => l.LanguageCode == lang)
                    .Select(l => l.Title)
                    .FirstOrDefault() ?? string.Empty,
                r.Film.Slug,
                r.Film.PosterPath
            ))
            .ToListAsync(ct);
    }

    internal async Task<int> GetReviewsCountAsync(CancellationToken ct)
    {
        return await db.Reviews
            .CountAsync(ct);
    }

    internal async Task<bool> FilmExistsAsync(int externalId, CancellationToken ct)
    {
        return await db.Films.AnyAsync(m => m.ExternalId == externalId, ct);
    }
}