using JetBrains.Annotations;

namespace Infrastructure.Services.Tmdb.Models;

[UsedImplicitly]
public sealed record TmdbMultiSearchResponse
{
    public int Page { get; init; }
    public IReadOnlyList<TmdbMultiSearchItem> Results { get; init; } = [];
    public int TotalPages { get; init; }
    public int TotalResults { get; init; }
}

[UsedImplicitly]
public sealed record TmdbMultiSearchItem
{
    public int Id { get; init; }
    public string MediaType { get; init; } = string.Empty; 

    public string? Title { get; init; }          // movie
    public string? Name { get; init; }           // tv / person
    public string? Overview { get; init; }
    public string? PosterPath { get; init; }
    public string? BackdropPath { get; init; }
    public string? ReleaseDate { get; init; }    // movie  "yyyy-MM-dd"
    public float VoteAverage { get; init; }
    public int VoteCount { get; init; }
    public float Popularity { get; init; }
    public int[] GenreIds { get; init; } = [];
    public string? OriginalLanguage { get; init; }

    // person
    public string? ProfilePath { get; init; }
    public string? KnownForDepartment { get; init; }
    public IReadOnlyList<TmdbMultiSearchItem> KnownFor { get; init; } = [];
}

// /search/movie
[UsedImplicitly]
public sealed record TmdbMovieSearchResponse
{
    public int Page { get; init; }
    public IReadOnlyList<TmdbMovieShortResponse> Results { get; init; } = [];
    public int TotalPages { get; init; }
    public int TotalResults { get; init; }
}

// /search/person
[UsedImplicitly]
public sealed record TmdbPersonSearchResponse
{
    public int Page { get; init; }
    public IReadOnlyList<TmdbPersonSearchItem> Results { get; init; } = [];
    public int TotalPages { get; init; }
    public int TotalResults { get; init; }
}

[UsedImplicitly]
public sealed record TmdbPersonSearchItem
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? ProfilePath { get; init; }
    public string? KnownForDepartment { get; init; }
    public float Popularity { get; init; }
    public IReadOnlyList<TmdbMovieShortResponse> KnownFor { get; init; } = [];
}