using JetBrains.Annotations;
using lumires.Domain.Enums;

namespace lumires.Core.Models;

[UsedImplicitly]
public record ExternalFilm(
    int ExternalId,
    string Title,
    string? Overview,
    string? PosterPath,
    float VoteAverage,
    int VoteCount,
    float Popularity,
    int Runtime,
    string ProductionCompany,
    string? BackdropPath,
    DateOnly ReleaseDate,
    string? TrailerUrl,
    string? Tagline,
    ExternalGenres Genres,
    IReadOnlyCollection<ExternalCastMember> TopCast,
    IReadOnlyCollection<ExternalDirector> Directors
);

[UsedImplicitly]
public record ExternalCastMember(
    int Id,
    string Name,
    string Character,
    int Order
);

[UsedImplicitly]
public record ExternalDirector(
    int Id,
    string Name
);

[UsedImplicitly]
public record ExternalGenreItem(
    int ExternalId,
    string Name
);

[UsedImplicitly]
public record ExternalGenres(
    IReadOnlyCollection<ExternalGenreItem> Items
);

[UsedImplicitly]
public record ExternalFilmShort(
    int ExternalId,
    string Title,
    string? PosterPath,
    int ReleaseYear,
    float VoteAverage,
    int VoteCount,
    float Popularity
);

[UsedImplicitly]
public class ExternalPerson
{
    public int ExternalId { get; init; }
    public required string Name { get; init; }
    public string Biography { get; init; } = string.Empty;
    public DateOnly? Birthday { get; init; }
    public DateOnly? Deathday { get; init; }
    public GenderType Gender { get; init; }
    public string? PlaceOfBirth { get; init; }
    public string? ProfilePath { get; init; }
}