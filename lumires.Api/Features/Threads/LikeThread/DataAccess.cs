using Ardalis.Result;
using JetBrains.Annotations;
using lumires.Core.Abstractions.Data;
using lumires.Core.Abstractions.Services;
using lumires.Core.Messaging;
using lumires.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace lumires.Api.Features.Threads.LikeThread;

[UsedImplicitly]
internal class DataAccess(
    IAppDbContext db,
    ICurrentUserService currentUserService,
    INotificationService notificationService) : IDataAccess
{

    internal async Task<Result<Response>> ToggleLikeAsync(Guid threadId, CancellationToken ct)
    {
        var thread = await db.Threads
            .Include(r => r.Likes)
            .Include(t => t.User)
            .ThenInclude(u => u.UserSettings)
            .FirstOrDefaultAsync(r => r.Id == threadId, ct);

        if (thread is null) return Result.NotFound();

        var currentUserId = currentUserService.UserId;
        var currentUser = await db.Users
            .Where(x => x.Id == currentUserService.UserId)
            .Select(x => new
            {
                x.Username,
                x.AvatarUrl
            }).FirstAsync(ct);

        var isLiked = thread.ToggleLike(currentUserId);

        if (isLiked && thread.User.UserSettings.Notifications.LikesOnContent)
        {
            var message = new NotificationMessage(NotificationType.LikedThread, currentUserId.ToString(),
                currentUser.Username,
                currentUser.AvatarUrl,
                thread.Id.ToString(),
                thread.Title,
                DateTime.UtcNow);

            notificationService.SendToUser(thread.UserId, message);
        }


        await db.SaveChangesAsync(ct);

        var response = new Response(isLiked, thread.LikesCount);
        return Result.Success(response);
    }
}