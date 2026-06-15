using Ardalis.Result;
using JetBrains.Annotations;
using lumires.Core.Abstractions.Data;
using Microsoft.EntityFrameworkCore;

namespace lumires.Api.Features.FilmsLists.AddFilmToList;

[UsedImplicitly]
internal class DataAccess(IAppDbContext db) : IDataAccess
{
    internal async Task<Result> AddFilmAsync(Command command, Guid userId, CancellationToken ct)
    {
        var list = await db.FilmsLists
            .Include(l => l.Films)
            .FirstOrDefaultAsync(l => l.Id == command.ListId, ct);

        if (list is null)
            return Result.NotFound();

        if (list.UserId != userId)
            return Result.Forbidden();

        var film = await db.Films
            .FirstOrDefaultAsync(f => f.ExternalId == command.FilmId, ct);

        if (film is null)
            return Result.NotFound();

        list.AddFilm(film.Id);

        await db.SaveChangesAsync(ct);

        return Result.Success();
    }
}
