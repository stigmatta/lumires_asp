using Ardalis.Result;
using JetBrains.Annotations;
using lumires.Core.Abstractions.Data;
using Microsoft.EntityFrameworkCore;

namespace lumires.Api.Features.Films.DeleteSavedFilm;

[UsedImplicitly]
internal class DataAccess(IAppDbContext db) : IDataAccess
{
    internal async Task<Result> DeleteSavedFilmAsync(Command command, Guid userId, CancellationToken ct)
    {
        var savedFilm =
            await db.SavedFilms.FirstOrDefaultAsync(f => f.Film.ExternalId == command.FilmId && f.UserId == userId, ct);

        if (savedFilm is null) return Result.NoContent();

        db.SavedFilms.Remove(savedFilm);
        await db.SaveChangesAsync(ct);

        return Result.NoContent();
    }
}