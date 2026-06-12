using Ardalis.Result;
using JetBrains.Annotations;
using lumires.Core.Abstractions.Data;
using lumires.Core.Auth;
using Microsoft.EntityFrameworkCore;

namespace lumires.Api.Features.Threads.DeleteThread;

[UsedImplicitly]
internal class DataAccess(IAppDbContext db) : IDataAccess
{
    internal async Task<Result> DeleteThreadAsync(Command command, Guid userId, string userRole, CancellationToken ct)
    {
        var thread =
            await db.Threads.FirstOrDefaultAsync(r => r.Id == command.ThreadId, ct);

        if (thread is null) return Result.NoContent();
        
        if (thread.UserId != userId || userRole == UserRoles.Admin || userRole == UserRoles.Moderator)
            return Result.Forbidden();

        db.Threads.Remove(thread);
        await db.SaveChangesAsync(ct);

        return Result.NoContent();
    }
}