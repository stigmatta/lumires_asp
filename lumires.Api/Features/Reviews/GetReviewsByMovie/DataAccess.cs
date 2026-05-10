using JetBrains.Annotations;
using lumires.Api.Extensions;
using lumires.Core.Abstractions.Data;
using lumires.Core.Abstractions.Services;
using Microsoft.EntityFrameworkCore;

namespace lumires.Api.Features.Reviews.GetReviewsByMovie;

[UsedImplicitly]
internal class DataAccess(IAppDbContext db, ICurrentUserService currentUserService) : IDataAccess
{
    internal async Task<List<ReviewItemResponse>> GetReviewsAsync(Query query, CancellationToken ct)
    {
        var filter = Specifications.BuildFilter(query);
        var category = Specifications.BuildCategory(query);
        var sort = Specifications.BuildSort(query);

        var queryable = db.Reviews
            .Where(r => r.MovieId == query.MovieId)
            .ApplyFilter(filter)
            .ApplyFilter(category)
            .ApplySorting(sort)
            .ApplyPaging(query.Page, query.PageSize);

        var userId = currentUserService.UserId;


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
                userId != Guid.Empty && r.Likes.Any(l => l.UserId == userId)
            ))
            .ToListAsync(ct);
    }

    internal async Task<int> GetReviewsCountAsync(Query query, CancellationToken ct)
    {
        var filter = Specifications.BuildFilter(query);
        var category = Specifications.BuildCategory(query);

        return await db.Reviews
            .Where(r => r.MovieId == query.MovieId)
            .ApplyFilter(filter)
            .ApplyFilter(category)
            .CountAsync(ct);
    }
}