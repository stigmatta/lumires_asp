using Core.Messaging;

namespace Core.Abstractions.Services;

public interface INotificationService
{
    Task SendToUserAsync(Guid userId, NotificationMessage message);
}