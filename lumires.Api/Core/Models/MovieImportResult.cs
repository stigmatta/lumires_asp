using JetBrains.Annotations;

namespace lumires.Api.Core.Models;

[UsedImplicitly]
public record MovieImportResult(
    int ExternalId,
    string Title,
    string? Overview,
    string? PosterPath,
    DateTime? ReleaseDate,
    Uri? TrailerUrl 
);