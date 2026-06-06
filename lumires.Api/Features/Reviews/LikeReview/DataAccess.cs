using Ardalis.Result;
using JetBrains.Annotations;
using lumires.Core.Abstractions.Data;
using lumires.Core.Abstractions.Services;
using lumires.Core.Messaging;
using lumires.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace lumires.Api.Features.Reviews.LikeReview;

[UsedImplicitly]
internal class DataAccess(
    IAppDbContext db,
    ICurrentUserService currentUserService,
    INotificationService notificationService) : IDataAccess
{
    internal async Task<Result<Response>> ToggleLikeAsync(Guid reviewId, CancellationToken ct)
    {
        var review = await db.Reviews
            .Include(r => r.Likes)
            .FirstOrDefaultAsync(r => r.Id == reviewId, ct);

        if (review is null) return Result.NotFound();

        var currentUserId = currentUserService.UserId;
        var currentUsername = await currentUserService.GetUsernameAsync(ct);

        var isLiked = review.ToggleLike(currentUserId);

        if (isLiked)
        {
            var message = new NotificationMessage(NotificationType.LikedReview, currentUserId.ToString(),
                currentUsername,
                review.Id.ToString(),
                DateTime.UtcNow);

            notificationService.SendToUser(review.UserId, message);
        }


        await db.SaveChangesAsync(ct);

        var response = new Response(isLiked, review.LikesCount);
        return Result.Success(response);
    }
}