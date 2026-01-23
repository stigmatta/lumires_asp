namespace lumires.Api.Domain.Entities;

public sealed class Movie
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public required string Title { get; init; }
    public int Year { get; init; }
}