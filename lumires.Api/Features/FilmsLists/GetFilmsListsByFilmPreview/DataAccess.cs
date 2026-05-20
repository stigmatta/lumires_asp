using JetBrains.Annotations;
using lumires.Core.Abstractions.Data;
using Microsoft.EntityFrameworkCore;

namespace lumires.Api.Features.FilmsLists.GetFilmsListsByFilmPreview;

[UsedImplicitly]
internal class DataAccess(IAppDbContext db) : IDataAccess
{
    public async Task<Response> GetFilmListsByFilmIdAsync(
        int filmId,
        CancellationToken ct)
    {
        var items = await db.FilmsLists
            .Where(fl => fl.Films
                .Any(f => f.Film.ExternalId == filmId))
            .OrderByDescending(fl => fl.LikesCount)
            .Select(fl => new FilmsListsItems(
                fl.Films
                    .Where(f => f.Film.ExternalId == filmId)
                    .Select(f => new FilmListItem(f.Film.BackdropPath))
                    .Take(1)
                    .ToList(),
                fl.Title
            ))
            .Take(4)
            .ToListAsync(ct);

        return new Response(items);
    }
}