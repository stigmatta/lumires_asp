using Ardalis.Result;
using JetBrains.Annotations;
using lumires.Core.Abstractions.Data;
using lumires.Core.Abstractions.Services;
using lumires.Core.Messaging;
using lumires.Core.Resources;
using lumires.Domain.Entities;
using lumires.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace lumires.Api.Features.Reviews.CreateReviewComment;

[UsedImplicitly]
internal class DataAccess(
    IAppDbContext db,
    INotificationService notificationService,
    ICurrentUserService currentUserService,
    IStringLocalizer<SharedResource> localizer) : IDataAccess
{
    internal async Task<Result<Response>> CreateReviewCommentAsync(Command command, CancellationToken ct)
    {
        var currentUserId = currentUserService.UserId;
        var currentUsername = await currentUserService.GetUsernameAsync(ct);

        var review = await db.Reviews
            .Where(m => m.Id == command.ReviewId)
            .Select(x => new
            {
                x.Id,
                x.UserId,
                RepliesAllowed = x.Reviewer.UserSettings.Notifications.RepliesAndMentions
            })
            .FirstOrDefaultAsync(ct);

        if (review is null) return Result.NotFound();
        
        User? targetedUser = null;
        if (command.TargetedUserId.HasValue)
        {
            targetedUser = await db.Users
                .Include(u => u.UserSettings)
                .FirstOrDefaultAsync(u => u.Id == command.TargetedUserId.Value, ct);


            if (targetedUser is null)
                return Result.Invalid(new ValidationError("TargetedUserId", localizer["ValidationError_UserId_Invalid"]));
        }

        var reviewComment = new ReviewComment(
            review.UserId,
            command.ReviewId,
            command.Text,
            command.TargetedUserId,
            command.IsSpoilerFree);

        db.ReviewComments.Add(reviewComment);

        if (review.RepliesAllowed)
        {
            var message = new NotificationMessage(
                NotificationType.ReviewReplied,
                currentUserId.ToString(),
                currentUsername,
                reviewComment.Id.ToString(),
                DateTime.UtcNow);

            if (targetedUser is not null && targetedUser.UserSettings.Notifications.RepliesAndMentions)
            {
                notificationService.SendToUsers(review.UserId, targetedUser.Id, message);
            }
            else
            {
                notificationService.SendToUser(review.UserId, message);
            }
        }

        await db.SaveChangesAsync(ct);

        return new Response(
            reviewComment.Id,
            reviewComment.Text,
            reviewComment.CreatedAt,
            reviewComment.IsSpoilerFree);
    }

}