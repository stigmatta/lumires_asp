using Ardalis.Result;
using JetBrains.Annotations;
using lumires.Core.Abstractions.Data;
using Microsoft.EntityFrameworkCore;

namespace lumires.Api.Features.FilmsLists.RemoveFilmFromList;

[UsedImplicitly]
internal class DataAccess(IAppDbContext db) : IDataAccess
{
    internal async Task<Result> RemoveFilmAsync(Command command, Guid userId, CancellationToken ct)
    {
        var list = await db.FilmsLists
            .FirstOrDefaultAsync(l => l.Id == command.ListId, ct);

        if (list is null)
            return Result.NotFound();

        if (list.UserId != userId)
            return Result.Forbidden();

        var membership = await db.ListFilms
            .FirstOrDefaultAsync(
                lf => lf.FilmsListId == command.ListId && lf.Film.ExternalId == command.FilmId,
                ct);

        if (membership is not null)
        {
            db.ListFilms.Remove(membership);
            await db.SaveChangesAsync(ct);
        }

        return Result.Success();
    }
}
