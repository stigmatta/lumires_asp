using JetBrains.Annotations;

namespace Infrastructure.Services.Tmdb;

[UsedImplicitly]
public sealed record TmdbMovieResponse(
    int Id,
    string Title,
    string? Overview,
    string PosterPath,
    string? BackdropPath,
    DateTime ReleaseDate,
    VideoResponse? Videos
);

[UsedImplicitly]
public sealed record VideoResponse(IReadOnlyList<VideoItem> Results);

[UsedImplicitly]
public sealed record VideoItem(string Key, string Site, string Type);

[UsedImplicitly]
public sealed record TmdbPagedResponse<T>
{
    public int Page { get; init; }
    [UsedImplicitly]
    public List<T> Results { get; init; } = [];
    public int TotalPages { get; init; }
    public int TotalResults { get; init; }
}

[UsedImplicitly]
public sealed record TmdbMovieShortResponse
{
    public int Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Overview { get; init; } = string.Empty;
    public string ReleaseDate { get; init; } = string.Empty;
    public string? PosterPath { get; init; }
    public string? BackdropPath { get; init; }
}