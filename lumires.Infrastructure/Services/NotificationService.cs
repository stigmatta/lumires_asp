using Core.Abstractions.Services;
using Core.Messaging;
using Infrastructure.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace Infrastructure.Services;

public sealed class NotificationService(
    IHubContext<NotificationHub, INotificationClient> hubContext
) : INotificationService
{
    public async Task SendToUserAsync(Guid userId, NotificationMessage message)
    {
        await hubContext.Clients
            .User(userId.ToString())
            .ReceiveNotification(message);
    }
}