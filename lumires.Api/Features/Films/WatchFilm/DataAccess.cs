using Ardalis.Result;
using JetBrains.Annotations;
using lumires.Core.Abstractions.Data;
using lumires.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace lumires.Api.Features.Films.WatchFilm;

[UsedImplicitly]
internal class DataAccess(IAppDbContext db) : IDataAccess
{
    internal async Task<Result<Response>> ToggleWatchedAsync(Command command, Guid userId, CancellationToken ct)
    {
        var existingFilm = await db.Films.FirstOrDefaultAsync(f => f.ExternalId == command.FilmId, ct);

        if (existingFilm is null) return Result.NotFound();

        var watchedFilm = await db.WatchedFilms
            .FirstOrDefaultAsync(f => f.FilmId == existingFilm.Id && f.UserId == userId, ct);

        bool isWatched;
        if (watchedFilm is null)
        {
            db.WatchedFilms.Add(new WatchedFilm(userId, existingFilm.Id));
            isWatched = true;
        }
        else
        {
            db.WatchedFilms.Remove(watchedFilm);
            isWatched = false;
        }

        await db.SaveChangesAsync(ct);

        return Result.Success(new Response(isWatched));
    }
}
