using JetBrains.Annotations;

namespace Infrastructure.Services.Tmdb.Models;

[UsedImplicitly]
public sealed record TmdbMovieResponse
{
    public int Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string? Overview { get; init; }
    public string PosterPath { get; init; } = string.Empty;
    public float VoteAverage { get; init; }
    public int VoteCount { get; init; }
    public float Popularity { get; init; }
    public string? BackdropPath { get; init; }
    public DateOnly ReleaseDate { get; init; }
    public int Runtime { get; init; }
    public string Tagline { get; init; } = string.Empty;
    public VideoResponse? Videos { get; init; }
    public IReadOnlyCollection<GenreResponse> Genres { get; init; } = [];
    public CreditsResponse? Credits { get; init; }
    public IReadOnlyCollection<TmdbProductionCompanyItem> ProductionCompanies { get; init; } = [];
}

[UsedImplicitly]
public sealed record VideoResponse
{
    public IReadOnlyList<VideoItem> Results { get; init; } = [];
}

[UsedImplicitly]
public sealed record VideoItem
{
    public string Key { get; init; } = string.Empty;
    public string Site { get; init; } = string.Empty;
    public string Type { get; init; } = string.Empty;
}

[UsedImplicitly]
public sealed record TmdbMovieShortResponse
{
    public int Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public float VoteAverage { get; init; }
    public int VoteCount { get; init; }
    public DateOnly ReleaseDate { get; init; }
    public string? PosterPath { get; init; }
    public float Popularity { get; init; }
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

[UsedImplicitly]
public sealed record TmdbProductionCompanyItem
{
    public int Id { get; init; }
    public string LogoPath { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string OriginCountry { get; init; } = string.Empty;
}