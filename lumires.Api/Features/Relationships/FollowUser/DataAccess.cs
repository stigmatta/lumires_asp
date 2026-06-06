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
        string username,
        CancellationToken ct)
    {
        var targetId = command.TargetUserId;

        if (userId == targetId)
            return Result.Conflict();

        var targetUser = await db.Users
            .AnyAsync(x => x.Id == targetId, ct);

        if (!targetUser)
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

        var relationship = new UsersRelationship(
            userId,
            targetId,
            UserRelationshipType.Follow,
            UserRelationshipStatus.Accepted);

        db.Relationships.Add(relationship);
        await db.SaveChangesAsync(ct);

        if (reversedFollow)
            notificationService.SendToUser(
                targetId,
                new NotificationMessage(
                    NotificationType.FollowedBack,
                    userId.ToString(),
                    username,
                    targetId.ToString(),
                    DateTime.UtcNow));
        else
            notificationService.SendToUser(
                targetId,
                new NotificationMessage(
                    NotificationType.Followed,
                    userId.ToString(),
                    username,
                    targetId.ToString(),
                    DateTime.UtcNow));

        return Result.Created(relationship.Id);
    }
}