using System.Linq.Expressions;
using JetBrains.Annotations;
using lumires.Api.Extensions;
using lumires.Core.Abstractions.Data;
using lumires.Core.Constants;
using lumires.Domain.Entities;
using lumires.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace lumires.Api.Features.Reviews.GetReviews;

[UsedImplicitly]
internal class DataAccess(IAppDbContext db) : IDataAccess
{
    private const string DefLang = LocalizationConstants.DefaultCulture;

    internal async Task<List<ReviewItemResponse>> GetReviewsAsync(Query query, string lang, Guid userId,
        CancellationToken ct)
    {
        Expression<Func<Review, bool>> filter;

        if (query.Category == ContentFilterEnum.FromFriends)
        {
            var friendIds = await db.Relationships
                .Where(f => f.SourceUserId == userId
                            && f.Type == UserRelationshipType.Follow
                            && f.Status == UserRelationshipStatus.Accepted)
                .Select(f => f.TargetUserId)
                .ToListAsync(ct);

            filter = Specifications.BuildFilter(query, friendIds);
        }
        else
        {
            filter = Specifications.BuildFilter(query);
        }

        var sort = Specifications.BuildSort(query);

        var queryable = db.Reviews
            .ApplyFilter(filter)
            .ApplySorting(sort)
            .ApplyPaging(query.Page, query.PageSize);

        return await queryable
            .Select(r => new ReviewItemResponse(
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

    internal async Task<int> GetReviewsCountAsync(Query query, CancellationToken ct)
    {
        var filter = Specifications.BuildFilter(query);

        return await db.Reviews
            .ApplyFilter(filter)
            .CountAsync(ct);
    }
}