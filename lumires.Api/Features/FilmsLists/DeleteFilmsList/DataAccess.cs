using Ardalis.Result;
using JetBrains.Annotations;
using lumires.Core.Abstractions.Data;
using lumires.Core.Auth;
using Microsoft.EntityFrameworkCore;

namespace lumires.Api.Features.FilmsLists.DeleteFilmsList;

[UsedImplicitly]
internal class DataAccess(IAppDbContext db) : IDataAccess
{
    internal async Task<Result> DeleteListAsync(Command command, Guid userId, string userRole, CancellationToken ct)
    {
        var list =
            await db.FilmsLists.FirstOrDefaultAsync(r => r.Id == command.ListId, ct);

        if (list is null) return Result.NoContent();
        
        if (list.UserId != userId || userRole == UserRoles.Admin || userRole == UserRoles.Moderator)
            return Result.Forbidden();

        db.FilmsLists.Remove(list);
        await db.SaveChangesAsync(ct);

        return Result.NoContent();
    }
}