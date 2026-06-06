namespace lumires.Domain.Entities;

public class UserThreadCommentLike
{
    public Guid UserThreadCommentId { get; init; }
    public Guid UserId { get; init; }
    public DateTimeOffset LikedAt { get; init; }
}