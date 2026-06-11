using Ardalis.Result;
using JetBrains.Annotations;
using lumires.Core.Abstractions.Data;
using Microsoft.EntityFrameworkCore;

namespace lumires.Api.Features.Threads.UpdateThread;

[UsedImplicitly]
internal class DataAccess(IAppDbContext db) : IDataAccess
{
    internal async Task<Result<Guid>> UpdateThreadAsync(Command command, Guid userId, CancellationToken ct)
    {
        var thread = await db.Threads
            .Where(t => t.Id == command.ThreadId)
            .FirstOrDefaultAsync(ct);

        if (thread is null || thread.Id == Guid.Empty) return Result.NotFound();
        
        if (thread.UserId != userId)
            return Result.Forbidden();

        thread.UpdateThread(command.Title, command.Text, command.Image, command.IsSpoilerFree);
        db.Threads.Update(thread);
        
        await db.SaveChangesAsync(ct);

        return Result.NoContent();
    }
}