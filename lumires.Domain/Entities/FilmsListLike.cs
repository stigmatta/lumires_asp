namespace lumires.Domain.Entities;

public sealed class FilmsListLike
{
    public Guid FilmsListId { get; init; }
    public Guid UserId { get; init; }
    public DateTimeOffset LikedAt { get; init; }
}