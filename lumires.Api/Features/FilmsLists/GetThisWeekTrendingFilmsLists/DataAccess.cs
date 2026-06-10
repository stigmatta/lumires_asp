using JetBrains.Annotations;
using lumires.Core.Abstractions.Data;
using Microsoft.EntityFrameworkCore;

namespace lumires.Api.Features.FilmsLists.GetThisWeekTrendingFilmsLists;

[UsedImplicitly]
internal class DataAccess(IAppDbContext db) : IDataAccess
{
    public async Task<Response> GetTrendingFilmListsWeekly(Guid currentUserId, CancellationToken ct)
    {
        var weekAgo = DateTime.UtcNow.AddDays(-7);

        var items = await db.FilmsLists
            .AsNoTracking()
            .Where(x => x.CreatedAt >= weekAgo && !x.IsPrivate)
            .OrderByDescending(x => x.Likes
                .Count(l => l.LikedAt >= weekAgo))
            .Take(6)
            .Select(x => new TrendingListItem(
                x.Id,
                x.Title,
                x.UserId,
                x.User.Username,
                x.Films.Count,
                x.Likes.Any(l => l.UserId == currentUserId),
                x.SavedLists.Any(l => l.UserId == currentUserId),
                x.Films.Select(f => new FilmListItem(f.Film.PosterPath))
                    .ToArray()))
            .ToArrayAsync(ct);

        return new Response(items);
    }
}