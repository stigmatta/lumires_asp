using JetBrains.Annotations;
using lumires.Core.Abstractions.Data;
using Microsoft.EntityFrameworkCore;

namespace lumires.Api.Features.FilmsLists.GetThisWeekTrendingFilmsLists;

[UsedImplicitly]
internal class DataAccess(IAppDbContext db) : IDataAccess
{
    public async Task<Response> GetTrendingFilmListsWeekly(CancellationToken ct)
    {
        var weekAgo = DateTime.UtcNow.AddDays(-7);

        var items = await db.FilmsLists
            .AsNoTracking()
            .Where(x => x.CreatedAt >= weekAgo)
            .OrderByDescending(x => x.LikesCount) //TODO if we`d have listcomments - change strategy of calculation
            .Take(6)
            .Select(x => new TrendingListItem(
                x.Id,
                x.Title,
                x.UserId,
                x.User.Username,
                x.Films.Count,
                x.Films.Select(f => new FilmListItem(f.Film.PosterPath))
                    .ToArray()))
            .ToArrayAsync(ct);

        return new Response(items);
    }
}