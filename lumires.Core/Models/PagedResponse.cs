using JetBrains.Annotations;

namespace lumires.Core.Models;

[UsedImplicitly]
public sealed record PagedResponse<T>
{
    public int Page { get; init; }

    [UsedImplicitly] public List<T> Results { get; init; } = [];

    public int TotalPages { get; init; }
    public int TotalResults { get; init; }
}