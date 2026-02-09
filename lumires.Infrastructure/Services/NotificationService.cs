using Contracts.Abstractions;
using Contracts.Messaging;
using Infrastructure.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace Infrastructure.Services;

internal class NotificationService(
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