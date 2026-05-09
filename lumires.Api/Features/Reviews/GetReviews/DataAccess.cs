using JetBrains.Annotations;
using lumires.Api.Extensions;
using lumires.Core.Abstractions.Data;
using Microsoft.EntityFrameworkCore;

namespace lumires.Api.Features.Reviews.GetReviews;

[UsedImplicitly]
internal class DataAccess(IAppDbContext db) : IDataAccess
{
    internal async Task<List<ReviewItemResponse>> GetReviewsAsync(Query query, CancellationToken ct)
    {
        var filter = Specifications.BuildFilter(query);
        var sort = Specifications.BuildSort(query);

        return await db.Reviews
            .Where(r => r.MovieId == query.MovieId)
            .ApplyFilter(filter)
            .ApplySorting(sort)
            .ApplyPaging(query.Page, query.PageSize)
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
                r.CreatedAt
            ))
            .ToListAsync(ct);
    }

    internal async Task<int> GetReviewsCountAsync(CancellationToken ct)
    {
        return await db.Reviews.CountAsync(ct);
    }
}