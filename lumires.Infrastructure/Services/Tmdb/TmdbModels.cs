using JetBrains.Annotations;

namespace Infrastructure.Services.Tmdb;

[UsedImplicitly]
public sealed record TmdbMovieResponse(
    int Id,
    string Title,
    string? Overview,
    string? PosterPath,
    string? BackdropPath,
    DateTime ReleaseDate,
    VideoResponse? Videos
);

[UsedImplicitly]
public sealed record VideoResponse(IReadOnlyList<VideoItem> Results);

[UsedImplicitly]
public sealed record VideoItem(string Key, string Site, string Type);