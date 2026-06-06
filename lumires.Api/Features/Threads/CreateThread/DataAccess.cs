using Ardalis.Result;
using JetBrains.Annotations;
using lumires.Core.Abstractions.Data;
using lumires.Domain.Entities;

namespace lumires.Api.Features.Threads.CreateThread;

[UsedImplicitly]
internal class DataAccess(IAppDbContext db) : IDataAccess
{
    internal async Task<Result<Guid>> CreateThreadAsync(Command command, Guid userId, CancellationToken ct)
    {
        var thread = new UserThread(userId, command.Title, command.Image, command.Text, command.IsSpoilerFree);

        db.Threads.Add(thread);
        await db.SaveChangesAsync(ct);

        return thread.Id;
    }
}