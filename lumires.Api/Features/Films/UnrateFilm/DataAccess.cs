using Ardalis.Result;
using JetBrains.Annotations;
using lumires.Core.Abstractions.Data;
using Microsoft.EntityFrameworkCore;

namespace lumires.Api.Features.Films.UnrateFilm;

[UsedImplicitly]
internal class DataAccess(IAppDbContext db) : IDataAccess
{
    internal async Task<Result> UnrateFilmAsync(Command command, Guid userId, CancellationToken ct)
    {
        var filmId = await db.Films
            .Where(m => m.ExternalId == command.FilmId)
            .Select(m => m.Id)
            .FirstOrDefaultAsync(ct);

        if (filmId == Guid.Empty) return Result.NotFound();


        var userRating = await db.UserFilmRatings
            .Where(fr => fr.FilmId == filmId && fr.UserId == userId)
            .FirstOrDefaultAsync(ct);

        if (userRating is null) return Result.NoContent();

        db.UserFilmRatings.Remove(userRating);
        await db.SaveChangesAsync(ct);

        return Result.NoContent();
    }
}