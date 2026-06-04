using lumires.Core.Messaging;

namespace lumires.Core.Abstractions.Services;

public interface INotificationService
{
    void SendToUser(Guid userId, NotificationMessage message);
    void SendToUsers(Guid primaryUserId, Guid? secondaryUserId, NotificationMessage message);
}