using System.Text.Json.Serialization;
using JetBrains.Annotations;
using lumires.Domain.Enums;

namespace lumires.Core.Messaging;

[UsedImplicitly]
public record NotificationMessage(
    [property: JsonConverter(typeof(JsonStringEnumConverter))]
    NotificationType Type,
    string SenderId,
    string? SenderName,
    string? SenderAvatar,
    string? TargetId,
    string? TargetPayload,
    DateTime CreatedAt);