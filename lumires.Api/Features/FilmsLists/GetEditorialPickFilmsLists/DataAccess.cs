using JetBrains.Annotations;
using lumires.Core.Abstractions.Data;
using Microsoft.EntityFrameworkCore;

namespace lumires.Api.Features.FilmsLists.GetEditorialPickFilmsLists;

[UsedImplicitly]
internal class DataAccess(IAppDbContext db) : IDataAccess
{
    public async Task<Response> GetEditorialListsAsync(Guid userId, CancellationToken ct)
    {
        var items = await db.FilmsLists
            .AsNoTracking()
            .Where(x => x.IsEditorPick && !x.IsPrivate)
            .OrderByDescending(x => x.LikesCount)
            .Take(3)
            .Select(x => new EditorialListItem(
                x.Id,
                x.Title,
                x.UserId,
                x.User.Username,
                x.Films.Count,
                x.Likes.Any(l => l.UserId == userId),
                x.SavedLists.Any(l => l.UserId == userId),
                x.Films.Select(f => new FilmListItem(f.Film.PosterPath))
                    .Take(11)
                    .ToArray()))
            .ToArrayAsync(ct);

        return new Response(items);
    }
}