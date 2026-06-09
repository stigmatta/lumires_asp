using System.Linq.Expressions;
using JetBrains.Annotations;
using lumires.Api.Extensions;
using lumires.Core.Abstractions.Data;
using lumires.Domain.Entities;
using lumires.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace lumires.Api.Features.FilmsLists.GetUserLikedFilmsLists;

[UsedImplicitly]
internal class DataAccess(IAppDbContext db) : IDataAccess
{
    internal async Task<List<ListItemResponse>> GetListsAsync(Query query, Guid userId,
        CancellationToken ct)
    {
        var sort = Specifications.BuildSort(query);

        var queryable = db.FilmsLists
            .Where(fl => fl.UserId == userId)
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
                l.SavedLists.Any(x => x.UserId == userId),
                l.Films
                    .Select(f => new FilmListItem(f.Film.BackdropPath))
                    .Take(6)
                    .ToList()
            ))
            .ToListAsync(ct);
    }

    internal async Task<int> GetListsCountAsync(Guid userId, CancellationToken ct)
    {
        return await db.FilmsLists
            .Where(f => f.Likes.Any(l => l.UserId == userId))
            .CountAsync(ct);
    }
}