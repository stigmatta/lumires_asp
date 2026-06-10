using Ardalis.Result;
using JetBrains.Annotations;
using lumires.Core.Abstractions.Data;
using lumires.Core.Abstractions.Services;
using lumires.Core.Messaging;
using lumires.Domain.Entities;
using lumires.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace lumires.Api.Features.Relationships.FollowUser;

[UsedImplicitly]
internal class DataAccess(IAppDbContext db, INotificationService notificationService) : IDataAccess
{
    internal async Task<Result<Guid>> FollowUserAsync(
        Command command,
        Guid userId,
        CancellationToken ct)
    {
        var targetId = command.TargetUserId;

        if (userId == targetId)
            return Result.Conflict();

        var targetUser = await db.Users
            .Include(u => u.UserSettings)
            .FirstOrDefaultAsync(x => x.Id == targetId, ct);

        if (targetUser is null)
            return Result.NotFound();

        var alreadyFollowing = await db.Relationships.AnyAsync(r =>
                r.SourceUserId == userId &&
                r.TargetUserId == targetId &&
                r.Type == UserRelationshipType.Follow,
            ct);

        if (alreadyFollowing)
            return Result.NoContent();

        var blocked = await db.Relationships.AnyAsync(r =>
                r.Type == UserRelationshipType.Block &&
                (
                    (r.SourceUserId == userId && r.TargetUserId == targetId) ||
                    (r.SourceUserId == targetId && r.TargetUserId == userId)
                ),
            ct);

        if (blocked)
            return Result.Conflict();

        var reversedFollow = await db.Relationships.AnyAsync(r =>
                r.SourceUserId == targetId &&
                r.TargetUserId == userId &&
                r.Type == UserRelationshipType.Follow,
            ct);

        var status = targetUser.UserSettings.IsAnyoneCanFollow
            ? UserRelationshipStatus.Accepted
            : UserRelationshipStatus.Pending;

        var relationship = new UsersRelationship(
            userId,
            targetId,
            UserRelationshipType.Follow,
            status);

        db.Relationships.Add(relationship);
        await db.SaveChangesAsync(ct);

        var userInfo = await db.Users
            .Where(u => u.Id == userId)
            .Select(u => new
            {
                u.Username,
                u.AvatarUrl
            }).FirstOrDefaultAsync(ct);

        if (status == UserRelationshipStatus.Accepted)
        {
            var message = new NotificationMessage(
                reversedFollow ? NotificationType.FollowedBack : NotificationType.Followed,
                userId.ToString(),
                userInfo!.Username,
                userInfo.AvatarUrl,
                null,
                null,
                DateTime.UtcNow);

            if (targetUser.UserSettings.Notifications.NewFollower) notificationService.SendToUser(targetId, message);
        }
        else
        {
            var message = new NotificationMessage(
                NotificationType.Followed,
                userId.ToString(),
                userInfo!.Username,
                userInfo.AvatarUrl,
                null,
                null,
                DateTime.UtcNow);

            notificationService.SendToUser(targetId, message);
        }

        return Result.Created(relationship.Id);
    }
}