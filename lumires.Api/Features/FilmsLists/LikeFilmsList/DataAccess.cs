using Ardalis.Result;
using JetBrains.Annotations;
using lumires.Core.Abstractions.Data;
using lumires.Core.Abstractions.Services;
using lumires.Core.Messaging;
using lumires.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace lumires.Api.Features.FilmsLists.LikeFilmsList;

[UsedImplicitly]
internal class DataAccess(
    IAppDbContext db,
    ICurrentUserService currentUserService,
    INotificationService notificationService) : IDataAccess
{
    internal async Task<Result<Response>> ToggleLikeAsync(Guid listId, CancellationToken ct)
    {
        var list = await db.FilmsLists
            .Include(r => r.Likes)
            .FirstOrDefaultAsync(r => r.Id == listId, ct);

        if (list is null) return Result.NotFound();

        var currentUserId = currentUserService.UserId;
        var currentUsername = await currentUserService.GetUsernameAsync(ct);

        var isLiked = list.ToggleLike(currentUserId);

        if (isLiked && list.User.UserSettings.Notifications.LikesOnContent)
        {
            var message = new NotificationMessage(NotificationType.LikedFilmsList, currentUserId.ToString(),
                currentUsername,
                list.Id.ToString(),
                DateTime.UtcNow);

            notificationService.SendToUser(list.UserId, message);
        }


        await db.SaveChangesAsync(ct);

        var response = new Response(isLiked, list.LikesCount);
        return Result.Success(response);
    }
}