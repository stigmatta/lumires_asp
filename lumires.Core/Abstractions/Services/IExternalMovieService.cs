using Ardalis.Result;
using Core.Models;

namespace Core.Abstractions.Services;

public interface IExternalMovieService
{
    Task<Result<ExternalMovie>> GetMovieDetailsAsync(int movieId, string lang, CancellationToken ct = default);
}