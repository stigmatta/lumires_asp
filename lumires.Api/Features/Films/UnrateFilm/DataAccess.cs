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
        var userRating = await db.UserFilmRatings
            .Where(fr => fr.Film.ExternalId == command.FilmId && fr.UserId == userId)
            .FirstOrDefaultAsync(ct);

        if (userRating is null) return Result.NoContent();

        db.UserFilmRatings.Remove(userRating);
        await db.SaveChangesAsync(ct);

        return Result.NoContent();
    }
}