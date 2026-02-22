using lumires.Core.Messaging;

namespace lumires.Core.Abstractions.Services;

public interface INotificationService
{
    Task SendToUserAsync(Guid userId, NotificationMessage message);
}