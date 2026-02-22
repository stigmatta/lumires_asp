namespace lumires.Domain.Entities;

public sealed class Movie
{
    public Guid Id { get; init; } = Guid.CreateVersion7();
    public required int ExternalId { get; init; }
    public required int Year { get; init; }
    public required string PosterPath { get; init; }
    public string? BackdropPath { get; init; }
    public string? TrailerUrl { get; init; }
    public ICollection<MovieLocalization> Localizations { get; init; } = new List<MovieLocalization>();
}