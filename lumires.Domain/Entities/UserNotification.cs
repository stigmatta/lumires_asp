using lumires.Domain.Enums;

namespace lumires.Domain.Entities;

public sealed class UserNotification
{
    public Guid Id { get; init; } = Guid.CreateVersion7();
    public Guid UserId { get; init; }
    public NotificationType Type { get; init; }
    public string SenderId { get; init; } = null!;
    public string? TargetId { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? ReadAt { get; init; }
}