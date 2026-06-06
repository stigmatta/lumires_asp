using JetBrains.Annotations;
using lumires.Core.Abstractions.Data;
using Microsoft.EntityFrameworkCore;

namespace lumires.Api.Features.Reviews.GetTrendingReviewsByFilm;

[UsedImplicitly]
internal class DataAccess(IAppDbContext db) : IDataAccess
{
    internal async Task<Response?> GetTrendingReviewsByFilmAsync(int filmId, CancellationToken ct)
    {
        var weekAgo = DateTime.UtcNow.AddDays(-7);

        var items = await db.Reviews
            .AsNoTracking()
            .Where(r => r.Film.ExternalId == filmId)
            .OrderByDescending(x => x.Likes
                .Count(l => l.LikedAt >= weekAgo))
            .Take(3)
            .Select(r => new TrendingReviewItem(
                r.Id,
                r.Title,
                r.UserId,
                r.Reviewer.Username,
                r.LikesCount))
            .ToListAsync(ct);

        return new Response(items);
    }
}