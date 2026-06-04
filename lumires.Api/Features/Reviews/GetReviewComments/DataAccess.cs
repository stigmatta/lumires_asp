using JetBrains.Annotations;
using lumires.Api.Extensions;
using lumires.Api.Features.Reviews.GetReviews;
using lumires.Core.Abstractions.Data;
using lumires.Core.Abstractions.Services;
using Microsoft.EntityFrameworkCore;

namespace lumires.Api.Features.Reviews.GetReviewComments;

[UsedImplicitly]
internal class DataAccess(IAppDbContext db, ICurrentUserService currentUserService) : IDataAccess
{
    internal async Task<List<CommentItemResponse>> GetCommentsByReviewId(Query query, CancellationToken ct)
    {
        var currentUserId = currentUserService.UserId;
        
        var queryable = db.ReviewComments
            .Where(rc => rc.ReviewId == query.ReviewId)
            .ApplyPaging(query.Page, query.PageSize);

        return await queryable
            .Select(r => new CommentItemResponse(
                r.Id,
                r.UserId,
                r.Commentator.Username,
                r.Commentator.AvatarUrl,
                r.LikesCount,
                currentUserId != Guid.Empty && r.Likes.Any(l => l.UserId == currentUserId),
                r.IsSpoilerFree,
                r.CreatedAt,
                r.TargetedUserId,
                r.TargetedUser != null ? r.TargetedUser.Username : null
            ))
            .ToListAsync(ct);
    }
    
    internal async Task<int> GetReviewsCountAsync(Query query, CancellationToken ct)
    {
        return await db.ReviewComments
            .Where(r => r.ReviewId == query.ReviewId)
            .CountAsync(ct);
    }
}