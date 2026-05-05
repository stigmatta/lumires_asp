using Ardalis.Result;
using lumires.Core.Models;

namespace lumires.Core.Abstractions.Services;

public interface IExternalMovieService
{
    Task<Result<ExternalMovie>> GetMovieDetailsAsync(int movieId, string lang, CancellationToken ct = default);
    Task<Result> SyncRecentMoviesAsync(CancellationToken ct);
    Task<Result> SyncTrendingMoviesAsync(CancellationToken ct);
    Task<Result> SyncPopularMoviesAsync(CancellationToken ct);
    Task<Result> SyncGenresAsync(CancellationToken ct);
}