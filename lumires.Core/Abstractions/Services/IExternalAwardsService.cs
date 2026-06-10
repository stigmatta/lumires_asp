using Ardalis.Result;
using lumires.Core.Models;

namespace lumires.Core.Abstractions.Services;

public interface IExternalAwardsService
{
    /// <summary>
    ///     Retrieves the aggregate award counts (nominations and wins) for a person.
    ///     TMDB exposes no awards endpoint, so the data is sourced from the public
    ///     TMDB website awards page.
    /// </summary>
    Task<Result<PersonAwards>> GetPersonAwardsAsync(int personId, CancellationToken ct = default);
}
