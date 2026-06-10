using Ardalis.Result;
using JetBrains.Annotations;
using lumires.Core.Abstractions.Data;
using Microsoft.EntityFrameworkCore;

namespace lumires.Api.Features.Films.DeleteFromWatchlist;

[UsedImplicitly]
internal class DataAccess(IAppDbContext db) : IDataAccess
{
    internal async Task<Result> DeleteFromWatchlistAsync(Command command, Guid userId, CancellationToken ct)
    {
        var watchlistFilm =
            await db.WatchlistFilms.FirstOrDefaultAsync(f => f.Film.ExternalId == command.FilmId && f.UserId == userId, ct);

        if (watchlistFilm is null) return Result.NoContent();

        db.WatchlistFilms.Remove(watchlistFilm);
        await db.SaveChangesAsync(ct);

        return Result.NoContent();
    }
}
