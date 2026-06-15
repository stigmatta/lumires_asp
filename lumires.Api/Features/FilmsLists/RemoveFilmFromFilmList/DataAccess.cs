using Ardalis.Result;
using JetBrains.Annotations;
using lumires.Core.Abstractions.Data;
using Microsoft.EntityFrameworkCore;

namespace lumires.Api.Features.FilmsLists.RemoveFilmFromFilmList;

[UsedImplicitly]
internal class DataAccess(IAppDbContext db) : IDataAccess
{
    internal async Task<Result> RemoveFilmFromListAsync(
        Command command,
        Guid currentUserId,
        CancellationToken ct)
    {
        var list = await db.FilmsLists
            .Include(x => x.Films)
            .ThenInclude(x => x.Film)
            .FirstOrDefaultAsync(
                x => x.Id == command.ListId &&
                     x.UserId == currentUserId,
                ct);

        if (list is null)
            return Result.NotFound();

        var film = list.Films
            .FirstOrDefault(x => x.Film.ExternalId == command.FilmId);

        if (film is null)
            return Result.Success();

        list.RemoveFilm(film.Film);

        await db.SaveChangesAsync(ct);

        return Result.Success();
    }
}