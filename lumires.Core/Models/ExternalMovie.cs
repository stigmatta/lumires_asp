using JetBrains.Annotations;

namespace lumires.Core.Models;

[UsedImplicitly]
public record ExternalMovie(
    int ExternalId,
    string Title,
    string? Overview,
    string PosterPath,
    string? BackdropPath,
    DateTime ReleaseDate,
    string? TrailerUrl
);