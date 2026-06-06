using JetBrains.Annotations;
using lumires.Core.Abstractions.Data;
using Microsoft.EntityFrameworkCore;

namespace lumires.Api.Features.Threads.GetThisWeekTrendingThreads;

[UsedImplicitly]
internal class DataAccess(IAppDbContext db) : IDataAccess
{
    internal async Task<Response?> GetTrendingThreadsWeeklyAsync(CancellationToken ct)
    {
        var weekAgo = DateTime.UtcNow.AddDays(-7);

        var items = await db.Threads
            .AsNoTracking()
            .OrderByDescending(x =>
                x.Likes.Count(l => l.LikedAt >= weekAgo) +
                x.UserThreadComments.Count(c => c.CreatedAt >= weekAgo) * 2)
            .Take(6)
            .Select(x => new TrendingThreadItem(
                x.Id,
                x.Title,
                x.Image,
                x.UserId,
                x.User.Username,
                x.CreatedAt,
                x.UserThreadComments.Count
            ))
            .ToListAsync(ct);

        return new Response(items);
    }
}