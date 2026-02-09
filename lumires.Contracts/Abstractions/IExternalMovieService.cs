using Ardalis.Result;
using Contracts.Models;

namespace Contracts.Abstractions;

public interface IExternalMovieService
{
    Task<Result<ExternalMovie>> GetMovieDetailsAsync(int movieId, string lang, CancellationToken ct = default);
}