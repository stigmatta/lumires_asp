using Ardalis.Result;
using lumires.Core.Models;

namespace lumires.Core.Abstractions.Services;

public interface IExternalFilmService
{
    Task<Result<ExternalFilm>> GetFilmDetailsAsync(int filmId, string lang, CancellationToken ct = default);
    Task<Result> SyncRecentFilmsAsync(CancellationToken ct);
    Task<Result> SyncTrendingFilmsAsync(CancellationToken ct);
    Task<Result> SyncPopularFilmsAsync(CancellationToken ct);
    Task<Result> SyncGenresAsync(CancellationToken ct);
    Task<Result> SyncCredits(int batchSize, CancellationToken ct = default);
    Task<Result<IReadOnlyCollection<ExternalFilmShort>>> GetSimilarFilmsAsync(int movieId, string lang, CancellationToken ct);
}