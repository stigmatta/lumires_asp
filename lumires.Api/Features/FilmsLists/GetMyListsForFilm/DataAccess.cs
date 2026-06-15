using JetBrains.Annotations;
using lumires.Core.Abstractions.Data;
using Microsoft.EntityFrameworkCore;

namespace lumires.Api.Features.FilmsLists.GetMyListsForFilm;

[UsedImplicitly]
internal class DataAccess(IAppDbContext db) : IDataAccess
{
    internal async Task<Response> GetListsAsync(Query query, Guid userId,
        CancellationToken ct)
    {
        var lists = await db.FilmsLists
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new ListResponse(
                x.Id,
                x.Title,
                x.Films.Count,
                x.IsPrivate,
                x.Films.Any(f => f.Film.ExternalId == query.FilmId),
                x.Films
                    .OrderBy(f => f.Order)
                    .Select(f => new FilmListItem(f.Film.PosterPath))
                    .Take(3)
                    .ToArray()))
            .ToArrayAsync(ct);

        return new Response(lists);
    }
}
