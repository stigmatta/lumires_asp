using lumires.Api.Shared.Models;

namespace lumires.Api.Shared.Abstractions;

public interface INotificationClient
{
    Task ReceiveNotification(NotificationCommand command);
}