using Ardalis.Result;
using JetBrains.Annotations;
using lumires.Core.Abstractions.Data;
using lumires.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace lumires.Api.Features.Films.WatchFilm;

[UsedImplicitly]
internal class DataAccess(IAppDbContext db) : IDataAccess
{
    internal async Task<Result<Guid>> MarkWatchedAsync(Command command, Guid userId, CancellationToken ct)
    {
        var existingFilm = await db.Films.FirstOrDefaultAsync(f => f.ExternalId == command.FilmId, ct);

        if (existingFilm is null) return Result.NotFound();
        var alreadyWatched =
            await db.WatchedFilms.AnyAsync(f => f.Film.ExternalId == command.FilmId && f.UserId == userId, ct);

        if (alreadyWatched) return Result.NoContent();

        var newLog = new WatchedFilm(userId, existingFilm.Id);
        db.WatchedFilms.Add(newLog);

        await db.SaveChangesAsync(ct);

        return newLog.Id; 
    }
}