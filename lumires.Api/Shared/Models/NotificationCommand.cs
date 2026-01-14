namespace lumires.Api.Shared.Models;

public record NotificationCommand(string Type, string SenderId, string? TargetId, DateTime CreatedAt);