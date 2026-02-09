using Contracts.Messaging;

namespace Contracts.Abstractions;

public interface INotificationService
{
    Task SendToUserAsync(Guid userId, NotificationMessage message);
}