using Ardalis.Result;
using JetBrains.Annotations;
using lumires.Core.Abstractions.Data;
using Microsoft.EntityFrameworkCore;

namespace lumires.Api.Features.Films.DeleteWatchedFilm;

[UsedImplicitly]
internal class DataAccess(IAppDbContext db) : IDataAccess
{
    internal async Task<Result> DeleteWatchedFilmAsync(Command command, Guid userId, CancellationToken ct)
    {
        var watchedFilm =
            await db.WatchedFilms.FirstOrDefaultAsync(f => f.Film.ExternalId == command.FilmId && f.UserId == userId,
                ct);

        if (watchedFilm is null) return Result.NoContent();

        db.WatchedFilms.Remove(watchedFilm);
        await db.SaveChangesAsync(ct);

        return Result.NoContent();
    }
}