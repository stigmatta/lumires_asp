using lumires.Core.Abstractions.Services;
using TickerQ.Utilities.Base;

namespace Infrastructure.Services.Tmdb.BackgroundJobs;

public class SyncFilmsJobs(IExternalFilmService filmSyncService, ILogger<SyncFilmsJobs> logger)
{
    [TickerFunction("SyncTrendingFilms")]
    public async Task SyncTrendingFilms(
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Starting trending movies sync at {Time}", DateTime.UtcNow);

        await filmSyncService.SyncTrendingFilmsAsync(cancellationToken);

        logger.LogInformation("Trending movies sync completed at {Time}", DateTime.UtcNow);
    }

    [TickerFunction("SyncPopularFilms")]
    public async Task SyncPopularFilms(
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Starting top rated movies sync at {Time}", DateTime.UtcNow);

        await filmSyncService.SyncPopularFilmsAsync(cancellationToken);

        logger.LogInformation("Top rated movies sync completed at {Time}", DateTime.UtcNow);
    }

    [TickerFunction("SyncRecentFilms")]
    public async Task SyncRecentFilms(
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Starting recent released movies sync at {Time}", DateTime.UtcNow);

        await filmSyncService.SyncRecentFilmsAsync(cancellationToken);

        logger.LogInformation("Recent released movies sync completed at {Time}", DateTime.UtcNow);
    }

    [TickerFunction("SyncGenres")]
    public async Task SyncGenres(
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Starting genres  sync at {Time}", DateTime.UtcNow);

        await filmSyncService.SyncGenresAsync(cancellationToken);

        logger.LogInformation("Genres sync completed at {Time}", DateTime.UtcNow);
    }

    [TickerFunction("SyncCredits")]
    public async Task SyncCredits(
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Starting credits sync at {Time}", DateTime.UtcNow);

        await filmSyncService.SyncCredits(20, cancellationToken);

        logger.LogInformation("Credits sync completed at {Time}", DateTime.UtcNow);
    }
}