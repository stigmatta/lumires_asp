using JetBrains.Annotations;

namespace Infrastructure.Services.Tmdb;

[UsedImplicitly]
public sealed record TmdbMovieResponse(
    int Id,
    string Title,
    string? Overview,
    string PosterPath,
    float VoteAverage,
    int VoteCount,
    float Popularity,
    string? BackdropPath,
    DateOnly ReleaseDate,
    VideoResponse? Videos,
    IReadOnlyCollection<GenreResponse> Genres
);

[UsedImplicitly]
public sealed record VideoResponse(IReadOnlyList<VideoItem> Results);

[UsedImplicitly]
public sealed record VideoItem(string Key, string Site, string Type);

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

[UsedImplicitly]
public sealed record GenreResponse
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
}

[UsedImplicitly]
public sealed record TmdbGenresResponse
{
    public IReadOnlyCollection<GenreResponse> Genres { get; init; } = [];
}