using lumires.Domain.Enums;
using lumires.Domain.Exceptions;


namespace lumires.Domain.Entities;

public sealed class UserNotification
{
    public Guid Id { get; }
    public Guid UserId { get; }
    public NotificationType Type { get; }
    public string SenderId { get; }
    public string? TargetId { get; }
    public DateTime CreatedAt { get; }
    public DateTime? ReadAt { get; private set; }

    private UserNotification() { }

    public UserNotification(Guid userId, NotificationType type, string senderId, string? targetId = null)
    {
        if (userId == Guid.Empty)
            throw new NotificationValidationException("UserId is required");

        if (string.IsNullOrWhiteSpace(senderId))
            throw new NotificationValidationException("SenderId is required");

        Id = Guid.CreateVersion7();
        UserId = userId;
        Type = type;
        SenderId = senderId;
        TargetId = targetId;
        CreatedAt = DateTime.UtcNow;
    }

    public void MarkAsRead()
    {
        if (ReadAt.HasValue)
            throw new InvalidNotificationOperationException("Notification already read");

        ReadAt = DateTime.UtcNow;
    }
}