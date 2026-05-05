using lumires.Core.Abstractions.Services;
using TickerQ.Utilities.Base;

namespace Infrastructure.Services.Tmdb.BackgroundJobs;

public class SyncMoviesJobs(IExternalMovieService movieSyncService, ILogger<SyncMoviesJobs> logger)
{
    [TickerFunction("SyncTrendingMovies")]
    public async Task SyncTrendingMovies(
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Starting trending movies sync at {Time}", DateTime.UtcNow);

        await movieSyncService.SyncTrendingMoviesAsync(cancellationToken);

        logger.LogInformation("Trending movies sync completed at {Time}", DateTime.UtcNow);
    }

    [TickerFunction("SyncPopularMovies")]
    public async Task SyncPopularMovies(
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Starting top rated movies sync at {Time}", DateTime.UtcNow);

        await movieSyncService.SyncPopularMoviesAsync(cancellationToken);

        logger.LogInformation("Top rated movies sync completed at {Time}", DateTime.UtcNow);
    }

    [TickerFunction("SyncRecentMovies")]
    public async Task SyncRecentMovies(
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Starting recent released movies sync at {Time}", DateTime.UtcNow);

        await movieSyncService.SyncRecentMoviesAsync(cancellationToken);

        logger.LogInformation("Recent released movies sync completed at {Time}", DateTime.UtcNow);
    }
    
    [TickerFunction("SyncGenres")]
    public async Task SyncGenres(
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Starting genres  sync at {Time}", DateTime.UtcNow);

        await movieSyncService.SyncGenresAsync(cancellationToken);

        logger.LogInformation("Genres sync completed at {Time}", DateTime.UtcNow);
    }
}