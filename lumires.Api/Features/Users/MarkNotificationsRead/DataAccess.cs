using Ardalis.Result;
using JetBrains.Annotations;
using lumires.Core.Abstractions.Data;
using Microsoft.EntityFrameworkCore;

namespace lumires.Api.Features.Users.MarkNotificationsRead;

[UsedImplicitly]
internal class DataAccess(IAppDbContext db) : IDataAccess
{
    internal async Task MarkNotificationsRead(Command command, Guid userId, CancellationToken ct)
    {
        var notifications = await db.UserNotifications
            .Where(x =>
                x.UserId == userId &&
                command.Ids.Contains(x.Id))
            .ToListAsync(ct);

        foreach (var notification in notifications)
        {
            notification.MarkAsRead();
        }

        await db.SaveChangesAsync(ct);

        Result.Success();
    }
}
