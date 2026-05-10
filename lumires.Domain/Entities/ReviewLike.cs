namespace lumires.Domain.Entities;

public sealed class ReviewLike
{
    public Guid ReviewId { get; init; }
    public Guid UserId { get; init; }
}