using Ardalis.Result;
using lumires.Core.Models;

namespace lumires.Core.Abstractions.Services;

public interface IExternalMovieService
{
    Task<Result<ExternalMovie>> GetMovieDetailsAsync(int movieId, string lang, CancellationToken ct = default);
}