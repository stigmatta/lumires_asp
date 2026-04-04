using TickerQ.Utilities.Entities;
using TickerQ.Utilities.Interfaces.Managers;

namespace Infrastructure.Services.Tmdb.BackgroundJobs;

public class SyncScheduler(ICronTickerManager<CronTickerEntity> cronTickerManager)
{
    public async Task ScheduleAsync(CancellationToken ct)
    {
        await cronTickerManager.AddBatchAsync(
        [
            new CronTickerEntity
            {
                Function = "SyncTrendingMovies",
                Expression = "0 3 * * *",
                Retries = 2,
                RetryIntervals = [300, 900]
            },
            new CronTickerEntity
            {
                Function = "SyncPopularMovies",
                Expression = "0 4 * * 1",
                Retries = 2,
                RetryIntervals = [300, 900]
            }
        ], ct);
    }
}