using JetBrains.Annotations;

namespace Infrastructure.Services.Tmdb;

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

[UsedImplicitly]
public sealed record TmdbProductionCompanyItem
{
    public int Id { get; init; }
    public string LogoPath { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string OriginCountry { get; init; } = string.Empty;
}

[UsedImplicitly]
public sealed record CreditsResponse
{
    public IReadOnlyList<CastMember> Cast { get; init; } = [];
    public IReadOnlyList<CrewMember> Crew { get; init; } = [];
}

[UsedImplicitly]
public sealed record CastMember
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Character { get; init; }
    public int Order { get; init; }
    public bool Adult { get; init; }
}

[UsedImplicitly]
public sealed record CrewMember
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Job { get; init; }
    public string? Department { get; init; }
}