using Ardalis.Result;
using JetBrains.Annotations;
using lumires.Core.Abstractions.Data;
using lumires.Core.Abstractions.Services;
using lumires.Core.Messaging;
using lumires.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace lumires.Api.Features.Reviews.LikeReviewComment;

[UsedImplicitly]
internal class DataAccess(
    IAppDbContext db,
    ICurrentUserService currentUserService,
    INotificationService notificationService) : IDataAccess
{
    internal async Task<Result<Response>> ToggleLikeAsync(Guid reviewCommentId, CancellationToken ct)
    {
        var reviewComment = await db.ReviewComments
            .Include(r => r.Likes)
            .FirstOrDefaultAsync(r => r.Id == reviewCommentId, ct);

        if (reviewComment is null) return Result.NotFound();

        var currentUserId = currentUserService.UserId;
        var currentUsername = await currentUserService.GetUsernameAsync(ct);

        var isLiked = reviewComment.ToggleLike(currentUserId);

        if (isLiked)
        {
            var message = new NotificationMessage(NotificationType.LikedReviewComment, currentUserId.ToString(),
                currentUsername,
                reviewComment.Id.ToString(),
                DateTime.UtcNow);

            notificationService.SendToUser(reviewComment.UserId, message);
        }


        await db.SaveChangesAsync(ct);

        var response = new Response(isLiked, reviewComment.LikesCount);
        return Result.Success(response);
    }
}