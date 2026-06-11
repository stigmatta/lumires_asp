using Ardalis.Result;
using JetBrains.Annotations;
using lumires.Core.Abstractions.Data;
using lumires.Core.Abstractions.Services;
using lumires.Core.Constants;
using lumires.Core.Messaging;
using lumires.Core.Resources;
using lumires.Domain.Entities;
using lumires.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace lumires.Api.Features.Threads.UpdateThreadComment;

[UsedImplicitly]
internal class DataAccess(
    IAppDbContext db,
    INotificationService notificationService,
    ICurrentUserService currentUserService,
    IStringLocalizer<SharedResource> localizer) : IDataAccess
{
    internal async Task<Result> UpdateThreadCommentAsync(Command command, CancellationToken ct)
    {
        var currentUserId = currentUserService.UserId;
        var currentUser = await db.Users
            .Where(u => u.Id == currentUserId)
            .Select(u => new
            {
                u.Username,
                u.AvatarUrl
            }).FirstOrDefaultAsync(ct);

        var thread = await db.Threads
            .Where(t => t.Id == command.ThreadId)
            .Select(x => new
            {
                x.Id,
                x.UserId,
                RepliesAllowed = x.User.UserSettings.Notifications.RepliesAndMentions,
                x.Title
            })
            .FirstOrDefaultAsync(ct);
        
        if (thread is null) return Result.NotFound();
        
        User? targetedUser = null;
        if (command.TargetedUserId.HasValue)
        {
            targetedUser = await db.Users
                .FirstOrDefaultAsync(u => u.Id == command.TargetedUserId.Value, ct);

            if (targetedUser is null)
                return Result.Invalid(new ValidationError("TargetedUserId", localizer["ValidationError_UserId_Invalid"]));
        }
        var existingComment = await db.ThreadComments
            .FirstOrDefaultAsync(c => c.Id == command.ReplyId, ct);

        if (existingComment is null)
            return Result.NotFound();

        if (existingComment.UserId != currentUserId)
            return Result.Forbidden();
        
        existingComment.UpdateThreadComment(command.Text, command.TargetedUserId, command.IsSpoilerFree);
        db.ThreadComments.Update(existingComment);

        if (thread.RepliesAllowed)
        {
            var message = new NotificationMessage(
                NotificationType.ThreadReplied,
                currentUserId.ToString(),
                currentUser!.Username,
                currentUser.AvatarUrl,
                thread.Id.ToString(),
                thread.Title,
                DateTime.UtcNow);

            if (targetedUser is not null && targetedUser.UserSettings.Notifications.RepliesAndMentions)
            {
                notificationService.SendToUsers(thread.UserId, targetedUser.Id, message);
            }
            else
            {
                notificationService.SendToUser(thread.UserId, message);
            }
        }

        await db.SaveChangesAsync(ct);

        return Result.NoContent();
    }
}