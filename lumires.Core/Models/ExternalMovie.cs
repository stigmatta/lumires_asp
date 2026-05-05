using JetBrains.Annotations;

namespace lumires.Core.Models;

[UsedImplicitly]
public record ExternalMovie(
    int ExternalId,
    string Title,
    string? Overview,
    string PosterPath,
    float VoteAverage,
    int VoteCount,
    float Popularity,
    string? BackdropPath,
    DateOnly ReleaseDate,
    string? TrailerUrl,
    ExternalGenres Genres
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
public record ExternalMovieShort(
    int ExternalId,
    string Title,
    string? BackdropPath,
    float VoteAverage,
    int VoteCount,
    float Popularity
);