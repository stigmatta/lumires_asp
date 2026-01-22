namespace lumires.Api.Shared.Models;

public record NotificationMessage(string Type, string SenderId, string? TargetId, DateTime CreatedAt);