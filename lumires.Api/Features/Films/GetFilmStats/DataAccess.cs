using Ardalis.Result;
using JetBrains.Annotations;
using lumires.Core.Abstractions.Data;
using lumires.Core.Helpers;
using lumires.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace lumires.Api.Features.Films.GetFilmStats;

[UsedImplicitly]
internal class DataAccess(
    IAppDbContext db) : IDataAccess
{
    private async Task<float?> GetFriendsAverage(Guid filmId, Guid currentUserId, CancellationToken ct)
    {
        var friendIds = await db.Relationships
            .Where(f => f.SourceUserId == currentUserId
                        && f.Type == UserRelationshipType.Follow
                        && f.Status == UserRelationshipStatus.Accepted)
            .Select(f => f.TargetUserId)
            .ToListAsync(ct);

        if (friendIds.Count == 0) return 0;

        var average = await db.UserFilmRatings
            .Where(u => friendIds.Contains(u.UserId) && u.FilmId == filmId)
            .AverageAsync(u => (float?)u.Rating, ct);

        return average ?? null;
    }

    internal async Task<Result<Response>> GetFilmStats(int filmId, Guid currentUserId, CancellationToken ct)
    {
        var startOfWeek = DateTime.UtcNow.AddDays(-7);

        var rawStat = await db.Films
            .Where(f => f.ExternalId == filmId)
            .Select(f => new
            {
                f.VoteAverage,
                f.VoteCount,
                Ratings = f.UserRatings
                    .Select(ur => ur.Rating),
                RatingCount = f.UserRatings.Count,
                f.Id,
                ReviewsCount = f.Reviews.Count,
                ThisWeekReviewsCount = f.Reviews
                    .Count(r => r.CreatedAt >= startOfWeek)
            })
            .FirstOrDefaultAsync(ct);

        var friendsAverage = await GetFriendsAverage(rawStat.Id, currentUserId, ct);

        var (rating, _) = CalculateFilmRating.Handle(
            rawStat.VoteAverage,
            rawStat.VoteCount,
            rawStat.Ratings.Any() ? rawStat.Ratings.Average() : 0f,
            rawStat.RatingCount);

        return new Response(rating, rawStat.ReviewsCount, rawStat.ThisWeekReviewsCount, friendsAverage);
    }
}