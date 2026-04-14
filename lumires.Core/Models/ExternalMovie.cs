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
    string? TrailerUrl
);