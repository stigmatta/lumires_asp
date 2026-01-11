namespace lumires.Api.Features.Notifications;

public interface INotificationClient
{
    Task ReceiveNotification(NotificationCommand command);
    
}