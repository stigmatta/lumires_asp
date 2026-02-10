using Domain.Enums;
using JetBrains.Annotations;

namespace Core.Messaging;

[UsedImplicitly]
public record NotificationMessage(NotificationType Type, string SenderId, string? TargetId, DateTime CreatedAt);