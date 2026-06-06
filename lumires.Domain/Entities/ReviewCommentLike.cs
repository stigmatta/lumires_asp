namespace lumires.Domain.Entities;

public sealed class ReviewCommentLike
{
    public Guid ReviewCommentId { get; init; }
    public Guid UserId { get; init; }
    public DateTimeOffset LikedAt { get; init; }
}