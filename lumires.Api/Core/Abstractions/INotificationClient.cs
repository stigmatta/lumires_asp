using lumires.Api.Core.Models;

namespace lumires.Api.Core.Abstractions;

public interface INotificationClient
{
    Task ReceiveNotification(NotificationMessage message);
}