using Infrastructure.Hubs;
using lumires.Core.Messaging;
using Microsoft.AspNetCore.SignalR;

namespace Infrastructure.Services;

public sealed class NotificationService(
    IHubContext<NotificationHub, INotificationClient> hubContext)
{
    public async Task SendToOneUserAsync(Guid userId, NotificationMessage message)
    {
        await hubContext.Clients
            .User(userId.ToString())
            .ReceiveNotification(message);
    }

    public async Task SendToUsersAsync(Guid[] userIds, NotificationMessage message)
    {
        var stringIds = userIds.Select(id => id.ToString()).ToArray();
        await hubContext.Clients
            .Users(stringIds)
            .ReceiveNotification(message);
    }
}