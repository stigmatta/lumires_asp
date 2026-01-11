namespace lumires.Api.Hubs;

public interface INotificationClient
{
    Task ReceiveNotification(string message);
}