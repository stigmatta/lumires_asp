using JetBrains.Annotations;
using lumires.Core.Abstractions.Data;
using Microsoft.EntityFrameworkCore;

namespace lumires.Api.Features.FilmsLists.GetUserPopularLists;

[UsedImplicitly]
internal class DataAccess(IAppDbContext db) : IDataAccess
{
    internal async Task<Response?> GetListsAsync(Query query, Guid userId,
        CancellationToken ct)
    {
        var lists = await db.FilmsLists
            .Where(x => x.User.Username == query.Username)
            .OrderByDescending(x => x.LikesCount)
            .Take(6)
            .Select(x => new ListResponse(
                x.Id,
                x.Title,
                x.Films.Count,
                x.Likes.Any(l => l.UserId == userId),
                x.SavedLists.Any(l => l.UserId == userId),
                x.UserId,
                x.User.Username,
                x.Films.Select(f => new FilmListItem(f.Film.PosterPath))
                    .ToArray()))
            .ToArrayAsync(ct);

        return new Response(lists);
    }
}