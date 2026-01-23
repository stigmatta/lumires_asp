namespace lumires.Api.Core.Models;

public record NotificationMessage(string Type, string SenderId, string? TargetId, DateTime CreatedAt);