using System.Linq.Expressions;
using JetBrains.Annotations;
using lumires.Api.Extensions;
using lumires.Core.Abstractions.Data;
using lumires.Domain.Entities;
using lumires.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace lumires.Api.Features.FilmsLists.GetFilmsLists;

[UsedImplicitly]
internal class DataAccess(IAppDbContext db) : IDataAccess
{
    internal async Task<List<ListItemResponse>> GetListsAsync(Query query, Guid userId,
        CancellationToken ct)
    {
        Expression<Func<FilmsList, bool>> filter;

        if (query.Category == ContentFilterEnum.FriendsLists)
        {
            var friendIds = await db.Relationships
                .Where(f => f.SourceUserId == userId
                            && f.Type == UserRelationshipType.Follow
                            && f.Status == UserRelationshipStatus.Accepted)
                .Select(f => f.TargetUserId)
                .ToListAsync(ct);

            filter = Specifications.BuildFilter(query, friendIds);
        }
        else
        {
            filter = Specifications.BuildFilter(query);
        }

        var sort = Specifications.BuildSort(query);

        var queryable = db.FilmsLists
            .ApplyFilter(filter)
            .Where(x => !x.IsPrivate || x.UserId == userId)
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
                l.IsPrivate,
                l.UserId == userId,
                l.Films.Select(f => new FilmListItem(f.Film.BackdropPath)).Take(6).ToList()
            ))
            .ToListAsync(ct);
    }

    internal async Task<int> GetListsCountAsync(Query query, Guid currentUserId, CancellationToken ct)
    {
        var filter = Specifications.BuildFilter(query);

        return await db.FilmsLists
            .ApplyFilter(filter)
            .Where(x => !x.IsPrivate || x.UserId == currentUserId)
            .CountAsync(ct);
    }
}