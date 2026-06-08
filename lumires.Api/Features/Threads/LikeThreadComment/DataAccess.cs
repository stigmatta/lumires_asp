using Ardalis.Result;
using JetBrains.Annotations;
using lumires.Core.Abstractions.Data;
using lumires.Core.Abstractions.Services;
using lumires.Core.Messaging;
using lumires.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace lumires.Api.Features.Threads.LikeThreadComment;

[UsedImplicitly]
internal class DataAccess(
    IAppDbContext db,
    ICurrentUserService currentUserService,
    INotificationService notificationService) : IDataAccess
{
    internal async Task<Result<Response>> ToggleLikeAsync(Guid threadCommentId, CancellationToken ct)
    {
        var threadComment = await db.ThreadComments
            .Include(r => r.Likes)
            .FirstOrDefaultAsync(r => r.Id == threadCommentId, ct);

        if (threadComment is null) return Result.NotFound();

        var currentUserId = currentUserService.UserId;
        var currentUsername = await currentUserService.GetUsernameAsync(ct);

        var isLiked = threadComment.ToggleLike(currentUserId);

        if (isLiked && threadComment.Commentator.UserSettings.Notifications.LikesOnContent)
        {
            var message = new NotificationMessage(NotificationType.LikedThreadComment, currentUserId.ToString(),
                currentUsername,
                threadComment.Id.ToString(),
                DateTime.UtcNow);

            notificationService.SendToUser(threadComment.UserId, message);
        }


        await db.SaveChangesAsync(ct);

        var response = new Response(isLiked, threadComment.LikesCount);
        return Result.Success(response);
    }
}