using JetBrains.Annotations;
using lumires.Core.Abstractions.Data;
using Microsoft.EntityFrameworkCore;

namespace lumires.Api.Features.Users.GetTrendingUsersByReviewComments;

[UsedImplicitly]
internal class DataAccess(IAppDbContext db) : IDataAccess
{
    internal async Task<Response> GetTrendingUsers(CancellationToken ct)
    {
        var weekAgo = DateTimeOffset.UtcNow.AddDays(-7);

        var items = await db.Users
            .AsNoTracking()
            .Where(u => u.Reviews.Any(r => r.ReviewComments.Any(c => c.CreatedAt >= weekAgo)))
            .Select(u => new
            {
                User = u,
                TopReview = u.Reviews
                    .OrderByDescending(r => r.ReviewComments.Count(c => c.CreatedAt >= weekAgo))
                    .First()
            })
            .OrderByDescending(x => x.TopReview.ReviewComments.Count(c => c.CreatedAt >= weekAgo))
            .Take(10)
            .Select(x => new MemberItem(
                x.User.Id,
                x.User.Username,
                x.TopReview.Id,
                x.TopReview.Title,
                x.TopReview.ReviewComments.Count(c => c.CreatedAt >= weekAgo)
            ))
            .ToListAsync(ct);

        return new Response(items);
    }
}