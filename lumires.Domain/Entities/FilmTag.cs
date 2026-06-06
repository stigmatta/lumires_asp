namespace lumires.Domain.Entities;

public sealed class FilmTag
{
    public Guid FilmId { get; init; }
    public Film Film { get; init; } = null!;

    public Guid TagId { get; init; }
    public Tag Tag { get; init; } = null!;

    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
}