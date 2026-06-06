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
    DateOnly? ReleaseDate,
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
    int? ReleaseYear, // if null = unreleased
    float VoteAverage,
    int VoteCount,
    float Popularity,
    int[] GenreIds
);

[UsedImplicitly]
public record ExternalPerson(
    int ExternalId,
    string Name,
    string? Biography,
    DateOnly? Birthday,
    DateOnly? Deathday,
    GenderType Gender,
    string? PlaceOfBirth,
    string? ProfilePath,
    string? KnownForDepartment
);

[UsedImplicitly]
public record SearchResults(
    IReadOnlyList<ExternalFilmShort>? Films,
    IReadOnlyList<ExternalPersonShort>? Directors,
    IReadOnlyList<ExternalPersonShort>? Actors,
    int Page,
    int TotalPages
);

[UsedImplicitly]
public record ExternalPersonShort(
    int ExternalId,
    string Name,
    string? ProfilePath,
    string? KnownForDepartment,
    float Popularity,
    IReadOnlyList<ExternalFilmShort> KnownFor
);