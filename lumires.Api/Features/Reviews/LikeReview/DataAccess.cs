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
            .Include(r => r.Reviewer)
            .ThenInclude(r => r.UserSettings)
            .FirstOrDefaultAsync(r => r.Id == reviewId, ct);

        if (review is null) return Result.NotFound();

        var isLiked = review.ToggleLike(currentUserService.UserId);

        if (isLiked && review.Reviewer.UserSettings.Notifications.LikesOnContent)
        {
            var message = new NotificationMessage(
                NotificationType.LikedReview,
                currentUserService.UserId.ToString(),
                await currentUserService.GetUsernameAsync(ct),
                review.Id.ToString(),
                DateTime.UtcNow);

            notificationService.SendToUser(review.UserId, message);
        }

        await db.SaveChangesAsync(ct);

        var response = new Response(isLiked, review.LikesCount);
        return Result.Success(response);
    }
}