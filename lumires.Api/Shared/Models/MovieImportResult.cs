namespace lumires.Api.Shared.Models;

public record MovieImportResult(
    int ExternalId,
    string Title,
    string? Overview,
    string? PosterPath,
    DateTime? ReleaseDate,
    string? TrailerUrl = null
);