using Ardalis.Result;
using JetBrains.Annotations;
using lumires.Core.Abstractions.Data;
using lumires.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace lumires.Api.Features.Films.SaveFilm;

[UsedImplicitly]
internal class DataAccess(IAppDbContext db) : IDataAccess
{
    internal async Task<Result<Guid>> MarkSavedAsync(Command command, Guid userId, CancellationToken ct)
    {
        var existingFilm = await db.Films.FirstOrDefaultAsync(f => f.ExternalId == command.FilmId, ct);

        if (existingFilm is null) return Result.NotFound();
        var alreadySaved =
            await db.SavedFilms.AnyAsync(f => f.Film.ExternalId == command.FilmId && f.UserId == userId, ct);

        if (alreadySaved) return Result.NoContent();

        var newlySaved = new SavedFilm(userId, existingFilm.Id);
        db.SavedFilms.Add(newlySaved);

        await db.SaveChangesAsync(ct);

        return newlySaved.Id; 
    }
}