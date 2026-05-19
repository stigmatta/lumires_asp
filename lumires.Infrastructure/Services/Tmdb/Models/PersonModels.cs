using JetBrains.Annotations;

namespace Infrastructure.Services.Tmdb.Models;

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

[UsedImplicitly]
public sealed record TmdbPersonDetailResponse
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Biography { get; init; }
    public DateOnly? Birthday { get; init; }
    public DateOnly? Deathday { get; init; }
    public string? PlaceOfBirth { get; init; }
    public string? ProfilePath { get; init; }
    public string? Homepage { get; init; }
    public string? KnownForDepartment { get; init; }
    public float Popularity { get; init; }
    public int Gender { get; init; }
    public bool Adult { get; init; }
}