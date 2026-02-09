using JetBrains.Annotations;

namespace Contracts.Models;

[UsedImplicitly]
public record ExternalMovie(
    int ExternalId,
    string Title,
    string? Overview,
    string? PosterPath,
    DateTime ReleaseDate,
    Uri? TrailerUrl
);