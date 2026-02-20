using Domain.Enums;

namespace Domain.Entities;

public sealed class UserNotification
{
    public Guid Id { get; init; } = Guid.CreateVersion7();
    public required Guid UserId { get; init; }
    public required NotificationType Type { get; init; }
    public required string SenderId { get; init; }
    public string? TargetId { get; init; }
    public required DateTime CreatedAt { get; init; }
    public DateTime? ReadAt { get; init; }
}