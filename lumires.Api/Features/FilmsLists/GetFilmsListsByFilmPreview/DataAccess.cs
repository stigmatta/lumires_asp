using JetBrains.Annotations;
using lumires.Core.Abstractions.Data;
using Microsoft.EntityFrameworkCore;

namespace lumires.Api.Features.FilmsLists.GetFilmsListsByFilmPreview;

[UsedImplicitly]
internal class DataAccess(IAppDbContext db) : IDataAccess
{
    public async Task<Response> GetFilmListsByFilmIdAsync(
        int filmId,
        Guid userId,
        CancellationToken ct)
    {
        var items = await db.FilmsLists
            .Where(fl => fl.Films
                .Any(f => f.Film.ExternalId == filmId))
            .OrderByDescending(fl => fl.LikesCount)
            .Select(fl => new FilmsListsItems(
                fl.Id,
                fl.Likes.Any(x => x.UserId == userId),
                fl.SavedLists.Any(x => x.UserId == userId),
                fl.Films
                    .Where(f => f.Film.ExternalId == filmId)
                    .Select(f => new FilmInListItem(f.Film.BackdropPath))
                    .Take(6)
                    .ToList(),
                fl.Title
            ))
            .Take(4)
            .ToListAsync(ct);

        return new Response(items);
    }
}