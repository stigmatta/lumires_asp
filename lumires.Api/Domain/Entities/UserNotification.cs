namespace lumires.Api.Domain.Entities;

public sealed class UserNotification
{
    public Guid Id { get; init; }
    public Guid UserId { get; init; }
    public string Type { get; init; } = null!;
    public string SenderId { get; init; } = null!;
    public string? TargetId { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? ReadAt { get; init; }
}