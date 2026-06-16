using Ardalis.Result;
using JetBrains.Annotations;
using lumires.Core.Abstractions.Data;
using Microsoft.EntityFrameworkCore;

namespace lumires.Api.Features.Users.GetUserNotifications;

[UsedImplicitly]
internal class DataAccess(IAppDbContext db) : IDataAccess
{
    internal async Task<Result<Response>> GetUserNotifications(string username, Guid currentUserId, CancellationToken ct)
    {
        var weekAgo = DateTime.UtcNow.AddDays(-7);

        var user = await db.Users
            .Where(x => x.Username == username)
            .Select(x => new { x.Id })
            .FirstOrDefaultAsync(ct);
        
        if (user is null) return Result.NotFound();

        if (user.Id != currentUserId)
            return Result.Forbidden();

        var userNotifications = await db.UserNotifications
            .Where(x =>
                x.UserId == user.Id &&
                x.CreatedAt >= weekAgo)
            .Select(x => new NotificationItem(
                x.Id,
                x.Type,
                x.SenderId,
                x.SenderName,
                x.SenderAvatar,
                x.TargetId,
                x.TargetPayload,
                x.CreatedAt,
                x.ReadAt))
            .ToListAsync(ct);
        return new Response(userNotifications);
    }
}