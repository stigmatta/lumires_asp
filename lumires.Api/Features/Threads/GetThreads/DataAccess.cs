using System.Linq.Expressions;
using JetBrains.Annotations;
using lumires.Api.Extensions;
using lumires.Core.Abstractions.Data;
using lumires.Domain.Entities;
using lumires.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace lumires.Api.Features.Threads.GetThreads;

[UsedImplicitly]
internal class DataAccess(IAppDbContext db) : IDataAccess
{
    internal async Task<List<ThreadItemResponse>> GetThreadsAsync(Query query, Guid userId, CancellationToken ct)
    {
        Expression<Func<UserThread, bool>> filter;

        if (query.Category == ContentFilterEnum.FromFriends)
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


        var queryable = db.Threads
            .ApplyFilter(filter)
            .ApplySorting(sort)
            .ApplyPaging(query.Page, query.PageSize);

        return await queryable
            .Select(t => new ThreadItemResponse(
                t.Id,
                t.UserId,
                t.User.Username,
                t.User.AvatarUrl,
                t.UserThreadComments.Count,
                t.Title,
                t.Image,
                t.Text,
                t.LikesCount,
                t.CreatedAt,
                userId != Guid.Empty && t.Likes.Any(l => l.UserId == userId),
                t.IsSpoilerFree,
                t.UserThreadComments
                    .Where(c => c.TargetedUserId == t.UserId || c.TargetedUserId == null)
                    .OrderByDescending(c => c.LikesCount)
                    .Select(c => new ThreadCommentItemResponse(
                        c.Id,
                        c.UserId,
                        c.Commentator.Username,
                        c.Commentator.AvatarUrl,
                        c.Text,
                        c.LikesCount
                    ))
                    .FirstOrDefault()))
            .ToListAsync(ct);
    }

    internal async Task<int> GetThreadsCountAsync(Query query, CancellationToken ct)
    {
        var filter = Specifications.BuildFilter(query);

        return await db.Threads
            .ApplyFilter(filter)
            .CountAsync(ct);
    }
}