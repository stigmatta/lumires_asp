using lumires.Core.Messaging;

namespace Infrastructure.Hubs;

public interface INotificationClient
{
    Task ReceiveNotification(NotificationMessage message);
}