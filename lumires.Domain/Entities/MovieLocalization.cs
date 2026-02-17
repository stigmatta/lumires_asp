namespace Domain.Entities;

public sealed class MovieLocalization
{
    public Guid Id { get; init; } = Guid.CreateVersion7();
    public Guid MovieId { get; init; }
    public Movie Movie { get; init; } = null!;
    public required string LanguageCode { get; init; }
    public required string Title { get; init; }

    public required string? Description { get; init; }
    // public NpgsqlTsVector SearchVector { get; private set; } = null!;
}