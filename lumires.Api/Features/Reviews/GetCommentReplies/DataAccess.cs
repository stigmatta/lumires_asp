using JetBrains.Annotations;
using lumires.Api.Extensions;
using lumires.Core.Abstractions.Data;
using lumires.Core.Abstractions.Services;
using Microsoft.EntityFrameworkCore;

namespace lumires.Api.Features.Reviews.GetCommentReplies;

[UsedImplicitly]
internal class DataAccess(IAppDbContext db, ICurrentUserService currentUserService) : IDataAccess
{
    internal async Task<List<CommentItemResponse>> GetRepliesByCommentId(Query query, CancellationToken ct)
    {
        var currentUserId = currentUserService.UserId;

        var queryable = db.ReviewComments
            .Where(rc => rc.ParentCommentId == query.ReplyId)
            .OrderBy(rc => rc.CreatedAt)
            .ApplyPaging(query.Page, query.PageSize);

        return await queryable
            .Select(r => new CommentItemResponse(
                r.Id,
                r.UserId,
                r.Commentator.Username,
                r.Commentator.AvatarUrl,
                r.Text,
                r.LikesCount,
                r.Replies.Count,
                currentUserId != Guid.Empty && r.Likes.Any(l => l.UserId == currentUserId),
                r.IsSpoilerFree,
                r.CreatedAt,
                r.ParentCommentId,
                r.TargetedUserId,
                r.TargetedUser != null ? r.TargetedUser.Username : null
            ))
            .ToListAsync(ct);
    }

    internal async Task<int> GetRepliesCountAsync(Query query, CancellationToken ct)
    {
        return await db.ReviewComments
            .Where(r => r.ParentCommentId == query.ReplyId)
            .CountAsync(ct);
    }
}
