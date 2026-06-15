using Ardalis.Result;
using JetBrains.Annotations;
using lumires.Core.Abstractions.Data;
using Microsoft.EntityFrameworkCore;

namespace lumires.Api.Features.FilmsLists.AddFilmToFilmList;

[UsedImplicitly]
internal class DataAccess(IAppDbContext db) : IDataAccess
{
    internal async Task<Result<Guid>> AddFilmToListAsync(Command command, Guid userId, CancellationToken ct)
    {
        var list = await db.FilmsLists
            .Include(x => x.Films)
            .ThenInclude(x => x.Film)
            .FirstOrDefaultAsync(
                x => x.Id == command.ListId &&
                     x.UserId == userId,
                ct);

        if (list is null)
            return Result.NotFound();

        if (list.Films.Any(x => x.Film.ExternalId == command.FilmId))
            return Result.Success();

        var film = await db.Films
            .FirstOrDefaultAsync(x => x.ExternalId == command.FilmId, ct);

        if (film is null)
            return Result.NotFound();

        list.AddFilm(film);

        await db.SaveChangesAsync(ct);

        return Result.Success();
    }

    
}