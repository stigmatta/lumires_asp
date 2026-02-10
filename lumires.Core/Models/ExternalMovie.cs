using JetBrains.Annotations;

namespace Core.Models;

[UsedImplicitly]
public record ExternalMovie(
    int ExternalId,
    string Title,
    string? Overview,
    string? PosterPath,
    DateTime ReleaseDate,
    Uri? TrailerUrl
);