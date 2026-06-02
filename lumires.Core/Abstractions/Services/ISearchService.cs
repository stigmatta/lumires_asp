using Ardalis.Result;
using lumires.Core.Models;

namespace lumires.Core.Abstractions.Services;

public interface ISearchService
{
    /// <summary>Movies + directors + actors in one TMDB /search/multi call.</summary>
    Task<Result<SearchResults>> SearchAllAsync(string lang, string searchTerm, int page, CancellationToken ct);

    /// <summary>Only movies — uses /search/movie.</summary>
    Task<Result<IReadOnlyList<ExternalFilmShort>>> SearchFilmsAsync(string lang, string searchTerm, int page, CancellationToken ct);

    /// <summary>People filtered by KnownForDepartment == "Directing".</summary>
    Task<Result<IReadOnlyList<ExternalPersonShort>>> SearchDirectorsAsync(string lang, string searchTerm, int page, CancellationToken ct);

    /// <summary>People filtered by KnownForDepartment == "Acting".</summary>
    Task<Result<IReadOnlyList<ExternalPersonShort>>> SearchActorsAsync(string lang, string searchTerm, int page, CancellationToken ct);
}