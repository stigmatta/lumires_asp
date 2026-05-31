using Ardalis.Result;
using JetBrains.Annotations;
using lumires.Core.Abstractions.Data;
using lumires.Core.Abstractions.Services;
using lumires.Core.Messaging;
using lumires.Domain.Entities;
using lumires.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace lumires.Api.Features.Threads.CreateThreadComment;

[UsedImplicitly]
internal class DataAccess(
    IAppDbContext db,
    INotificationService notificationService,
    ICurrentUserService currentUserService) : IDataAccess
{
    internal async Task<Result<Response>> CreateThreadCommentAsync(Command command, CancellationToken ct)
    {
        var currentUserId = currentUserService.UserId;
        var currentUsername = await currentUserService.GetUsernameAsync(ct);

        var thread = await db.Threads.FirstOrDefaultAsync(m => m.Id == command.ThreadId, ct);
        if (thread is null) return Result.NotFound();

        var threadComment = new UserThreadComment(thread.UserId, command.ThreadId, command.Text, command.TargetedUserId,
            command.IsSpoilerFree);

        await db.ThreadComments.AddAsync(threadComment, ct);

        var message = new NotificationMessage(NotificationType.ThreadReplied, currentUserId.ToString(), currentUsername,
            threadComment.Id.ToString(), //TODO or thread.Id ?
            DateTime.UtcNow);

        await notificationService.SendToUsersAsync(thread.UserId, command.TargetedUserId, message);

        await db.SaveChangesAsync(ct);

        return new Response(threadComment.Id, threadComment.Text, threadComment.CreatedAt, threadComment.IsSpoilerFree);
    }
}