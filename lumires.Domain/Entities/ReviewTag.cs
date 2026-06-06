namespace lumires.Domain.Entities;

public sealed class ReviewTag
{
    public Guid ReviewId { get; init; }
    public Review Review { get; init; } = null!;

    public Guid TagId { get; init; }
    public Tag Tag { get; init; } = null!;

    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
}