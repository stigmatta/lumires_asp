using JetBrains.Annotations;

namespace lumires.Api.Infrastructure.Services.Tmdb;

public record TmdbMovieResponse(
    int Id,
    string Title,
    string? Overview,
    string? PosterPath,
    DateTime? ReleaseDate,
    VideoResponse? Videos
);
[UsedImplicitly]
public record VideoResponse(IReadOnlyList<VideoItem> Results);

[UsedImplicitly]
public record VideoItem(string Key, string Site, string Type);