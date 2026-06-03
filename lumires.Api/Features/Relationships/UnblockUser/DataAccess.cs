using Ardalis.Result;
using JetBrains.Annotations;
using lumires.Core.Abstractions.Data;
using lumires.Domain.Entities;
using lumires.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace lumires.Api.Features.Relationships.UnblockUser;

[UsedImplicitly]
internal class DataAccess(IAppDbContext db) : IDataAccess
{
    internal async Task<Result<Guid>> UnblockUserAsync(
        Command command,
        Guid userId,
        CancellationToken ct)
    {
        var targetId = command.TargetUserId;

        if (userId == targetId)
            return Result.Conflict();

        var targetUser = await db.Users
            .FirstOrDefaultAsync(x => x.Id == targetId, ct);

        if (targetUser is null)
            return Result.NotFound();

        var existing = await db.Relationships.FirstOrDefaultAsync(r =>
                r.SourceUserId == userId &&
                r.TargetUserId == targetId,
            ct);

        if (existing is not null && existing.Type == UserRelationshipType.Block)
        {
            db.Relationships.Remove(existing);
        }

        if (existing is not null && existing.Type != UserRelationshipType.Block)
        {
            return Result.NoContent();
        }


        await db.SaveChangesAsync(ct);
        return Result.NoContent();
    }
}