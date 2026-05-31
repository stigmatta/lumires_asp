using JetBrains.Annotations;
using lumires.Core.Abstractions.Data;
using lumires.Core.Abstractions.Services;
using Microsoft.EntityFrameworkCore;

namespace lumires.Api.Features.Reviews.GetReview;

[UsedImplicitly]
internal class DataAccess(IAppDbContext db, ICurrentUserService currentUserService) : IDataAccess
{
    internal async Task<Response?> GetReviewByIdAsync(Query query, CancellationToken ct)
    {
        var currentUserId = currentUserService.UserId;
        return await db.Reviews
            .Where(x => x.Id == query.ReviewId)
            .Select(x => new Response(
                x.Id,
                x.UserId,
                x.Reviewer.Username,
                x.Reviewer.AvatarUrl,
                x.ReviewComments.Count,
                x.Rating,
                x.Title,
                x.Text,
                x.LikesCount,
                x.CreatedAt,
                currentUserId != Guid.Empty && x.Likes.Any(l => l.UserId == currentUserId),
                x.IsSpoilerFree,
                x.ReviewComments.Select(c => new CommentItemResponse(
                    c.Id,
                    c.UserId,
                    c.Commentator.Username,
                    c.Commentator.AvatarUrl,
                    c.LikesCount,
                    currentUserId != Guid.Empty && c.Likes.Any(l => l.UserId == currentUserId),
                    c.IsSpoilerFree,
                    c.CreatedAt,
                    c.TargetedUserId,
                    c.TargetedUser != null ? c.TargetedUser.Username : null
                )).ToList()
            ))
            .FirstOrDefaultAsync(ct);
    }
}