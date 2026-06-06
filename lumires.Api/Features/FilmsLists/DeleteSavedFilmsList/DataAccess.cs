using Ardalis.Result;
using JetBrains.Annotations;
using lumires.Core.Abstractions.Data;
using Microsoft.EntityFrameworkCore;

namespace lumires.Api.Features.FilmsLists.DeleteSavedFilmsList;

[UsedImplicitly]
internal class DataAccess(IAppDbContext db) : IDataAccess
{
    internal async Task<Result> DeleteSavedListAsync(Command command, Guid userId, CancellationToken ct)
    {
        var savedList =
            await db.SavedLists.FirstOrDefaultAsync(f => f.ListId == command.ListId && f.UserId == userId, ct);

        if (savedList is null) return Result.NoContent();

        db.SavedLists.Remove(savedList);
        await db.SaveChangesAsync(ct);

        return Result.NoContent();
    }
}