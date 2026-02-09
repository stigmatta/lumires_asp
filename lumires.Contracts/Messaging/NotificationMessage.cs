using JetBrains.Annotations;
using lumires.Domain.Enums;

namespace Contracts.Messaging;

[UsedImplicitly]
public record NotificationMessage(NotificationType Type, string SenderId, string? TargetId, DateTime CreatedAt);