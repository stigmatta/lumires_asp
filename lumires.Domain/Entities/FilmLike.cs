namespace lumires.Domain.Entities;

public sealed class FilmLike
{
    public Guid FilmId { get; init; }
    public Guid UserId { get; init; }
    public DateTimeOffset LikedAt { get; init; }
}