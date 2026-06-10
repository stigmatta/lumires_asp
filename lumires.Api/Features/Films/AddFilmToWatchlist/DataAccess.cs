using Ardalis.Result;
using JetBrains.Annotations;
using lumires.Core.Abstractions.Data;
using lumires.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace lumires.Api.Features.Films.AddFilmToWatchlist;

[UsedImplicitly]
internal class DataAccess(IAppDbContext db) : IDataAccess
{
    internal async Task<Result<Guid>> AddToWatchlistAsync(Command command, Guid userId, CancellationToken ct)
    {
        var existingFilm = await db.Films.FirstOrDefaultAsync(f => f.ExternalId == command.FilmId, ct);

        if (existingFilm is null) return Result.NotFound();
        var alreadyAdded =
            await db.WatchlistFilms.AnyAsync(f => f.Film.ExternalId == command.FilmId && f.UserId == userId, ct);

        if (alreadyAdded) return Result.NoContent();

        var newEntry = new WatchlistFilm(userId, existingFilm.Id);
        db.WatchlistFilms.Add(newEntry);

        await db.SaveChangesAsync(ct);

        return newEntry.Id;
    }
}
