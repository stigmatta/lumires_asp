using JetBrains.Annotations;
using lumires.Api.Extensions;
using lumires.Core.Abstractions.Data;
using Microsoft.EntityFrameworkCore;

namespace lumires.Api.Features.FilmsLists.GetFilmsLists;

[UsedImplicitly]
internal class DataAccess(IAppDbContext db) : IDataAccess
{
    internal async Task<List<ListItemResponse>> GetListsAsync(Query query, Guid userId,
        CancellationToken ct)
    {
        var filter = Specifications.BuildFilter(query);
        var sort = Specifications.BuildSort(query);

        var queryable = db.FilmsLists
            .ApplyFilter(filter)
            .ApplySorting(sort)
            .ApplyPaging(query.Page, query.PageSize);

        return await queryable
            .Select(l => new ListItemResponse(
                l.Id,
                l.Title,
                l.UserId,
                l.User.Username,
                l.Films.Count,
                l.Likes.Any(x => x.UserId == userId),
                l.Films
                    .Select(f => new FilmListItem(f.Film.BackdropPath))
                    .Take(6)
                    .ToList()
            ))
            .ToListAsync(ct);
    }

    internal async Task<int> GetReviewsCountAsync(Query query, CancellationToken ct)
    {
        var filter = Specifications.BuildFilter(query);

        return await db.FilmsLists
            .ApplyFilter(filter)
            .CountAsync(ct);
    }
}