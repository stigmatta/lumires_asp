using lumires.Domain.Enums;
using lumires.Domain.Exceptions;

namespace lumires.Domain.Entities;

public sealed class UserNotification
{
    private UserNotification()
    {
    }

    public UserNotification(Guid userId, NotificationType type, string senderId, string? senderName,
        string? senderAvatar, string? targetId, string? targetPayload)
    {
        if (userId == Guid.Empty)
            throw new DomainException("UserId is required", nameof(UserId));

        if (string.IsNullOrWhiteSpace(senderId))
            throw new DomainException("SenderId is required", nameof(SenderId));

        Id = Guid.CreateVersion7();
        UserId = userId;
        Type = type;
        SenderId = senderId;
        SenderName = senderName;
        SenderAvatar = senderAvatar;
        TargetId = targetId;
        TargetPayload = targetPayload;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public Guid Id { get; }
    public Guid UserId { get; }
    public NotificationType Type { get; }
    public string SenderId { get; } = null!;
    public string? SenderName { get; private set; }
    public string? SenderAvatar { get; private set; }
    public string? TargetId { get; }
    public string? TargetPayload { get; }
    public DateTimeOffset CreatedAt { get; }
    public DateTimeOffset? ReadAt { get; private set; }

    public void MarkAsRead()
    {
        if (ReadAt.HasValue)
            throw new DomainException("Notification already read", nameof(ReadAt));

        ReadAt = DateTimeOffset.UtcNow;
    }
}