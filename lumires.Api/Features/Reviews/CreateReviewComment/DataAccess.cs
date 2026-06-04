using Ardalis.Result;
using JetBrains.Annotations;
using lumires.Core.Abstractions.Data;
using lumires.Core.Abstractions.Services;
using lumires.Core.Messaging;
using lumires.Domain.Entities;
using lumires.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace lumires.Api.Features.Reviews.CreateReviewComment;

[UsedImplicitly]
internal class DataAccess(
    IAppDbContext db,
    INotificationService notificationService,
    ICurrentUserService currentUserService) : IDataAccess
{
    internal async Task<Result<Response>> CreateReviewCommentAsync(Command command, CancellationToken ct)
    {
        var currentUserId = currentUserService.UserId;
        var currentUsername = await currentUserService.GetUsernameAsync(ct);

        var review = await db.Reviews.FirstOrDefaultAsync(m => m.Id == command.ReviewId, ct);
        if (review is null) return Result.NotFound();

        var reviewComment = new ReviewComment(review.UserId, command.ReviewId, command.Text, command.TargetedUserId,
            command.IsSpoilerFree);

        db.ReviewComments.Add(reviewComment);

        var message = new NotificationMessage(NotificationType.ReviewReplied, currentUserId.ToString(), currentUsername,
            reviewComment.Id.ToString(), //TODO or review.Id ?
            DateTime.UtcNow);

         notificationService.SendToUsers(review.UserId, command.TargetedUserId, message);

        await db.SaveChangesAsync(ct);

        return new Response(reviewComment.Id, reviewComment.Text, reviewComment.CreatedAt, reviewComment.IsSpoilerFree);
    }
}