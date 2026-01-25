using JetBrains.Annotations;

namespace lumires.Api.Infrastructure.Services.Tmdb;

[UsedImplicitly]
public sealed record TmdbMovieResponse(
    int Id,
    string Title,
    string? Overview,
    string? PosterPath,
    DateTime? ReleaseDate,
    VideoResponse? Videos
);

[UsedImplicitly]
public sealed record VideoResponse(IReadOnlyList<VideoItem> Results);

[UsedImplicitly]
public sealed record VideoItem(string Key, string Site, string Type);