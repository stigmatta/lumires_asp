using JetBrains.Annotations;
using lumires.Api.Extensions;
using lumires.Core.Abstractions.Data;
using Microsoft.EntityFrameworkCore;

namespace lumires.Api.Features.Reviews.GetReviewsByFilm;

[UsedImplicitly]
internal class DataAccess(IAppDbContext db) : IDataAccess
{
    internal async Task<List<ReviewItemResponse>> GetReviewsAsync(Query query, Guid userId, CancellationToken ct)
    {
        var filter = Specifications.BuildFilter(query);
        var category = Specifications.BuildCategory(query);
        var sort = Specifications.BuildSort(query);


        var queryable = db.Reviews
            .Where(r => r.Film.ExternalId == query.FilmId)
            .ApplyFilter(filter)
            .ApplyFilter(category)
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
                userId != Guid.Empty && r.Likes.Any(l => l.UserId == userId)
            ))
            .ToListAsync(ct);
    }

    internal async Task<int> GetReviewsCountAsync(Query query, CancellationToken ct)
    {
        var filter = Specifications.BuildFilter(query);
        var category = Specifications.BuildCategory(query);

        return await db.Reviews
            .Where(r => r.Film.ExternalId == query.FilmId)
            .ApplyFilter(filter)
            .ApplyFilter(category)
            .CountAsync(ct);
    }

    internal async Task<bool> FilmExistsAsync(int externalId, CancellationToken ct)
    {
        return await db.Films.AnyAsync(m => m.ExternalId == externalId, ct);
    }
}