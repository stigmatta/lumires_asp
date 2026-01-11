namespace lumires.Api.Features.Notifications;

public record NotificationCommand(string Type, string SenderId, string? TargetId, DateTime CreatedAt);