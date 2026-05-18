using JetBrains.Annotations;

namespace lumires.Core.Models;

[UsedImplicitly]
public sealed class PagedResponse<T>(List<T> results, int totalResults, int page, int pageSize)
{
    public List<T> Results { get; } = results;
    public int TotalResults { get; } = totalResults;
    public int Page { get; } = page;
    public int PageSize { get; } = pageSize;
    public int TotalPages => (int)Math.Ceiling((double)TotalResults / PageSize);
}