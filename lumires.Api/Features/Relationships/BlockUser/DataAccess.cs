using Ardalis.Result;
using JetBrains.Annotations;
using lumires.Core.Abstractions.Data;
using lumires.Domain.Entities;
using lumires.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace lumires.Api.Features.Relationships.BlockUser;

[UsedImplicitly]
internal class DataAccess(IAppDbContext db) : IDataAccess
{
    internal async Task<Result<Guid>> BlockUserAsync(
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

        if (existing is not null && existing.Type == UserRelationshipType.Block) return Result.NoContent();

        if (existing is not null && existing.Type == UserRelationshipType.Follow)
        {
            existing.SetType(UserRelationshipType.Block);
            db.Relationships.Update(existing);
        }
        else
        {
            var relationship = new UsersRelationship(
                userId,
                targetId,
                UserRelationshipType.Follow,
                UserRelationshipStatus.Accepted // TODO if we`d have private accounts
            );

            db.Relationships.Add(relationship);
        }


        await db.SaveChangesAsync(ct);
        return Result.NoContent();
    }
}