using JetBrains.Annotations;
using lumires.Domain.Enums;

namespace lumires.Core.Messaging;

[UsedImplicitly]
public record NotificationMessage(
    NotificationType Type,
    string SenderId,
    string? SenderName,
    string? TargetId,
    DateTime CreatedAt);