using Ardalis.Result;
using JetBrains.Annotations;
using lumires.Core.Abstractions.Data;
using lumires.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace lumires.Api.Features.Relationships.UnfollowUser;

[UsedImplicitly]
internal class DataAccess(IAppDbContext db) : IDataAccess
{
    internal async Task<Result> UnfollowUserAsync(
        Command command,
        Guid userId,
        CancellationToken ct)
    {
        var targetId = command.TargetUserId;

        if (userId == targetId)
            return Result.Conflict();

        var relationship = await db.Relationships.FirstOrDefaultAsync(r =>
                r.SourceUserId == userId &&
                r.TargetUserId == targetId &&
                r.Type == UserRelationshipType.Follow,
            ct);

        if (relationship is null)
            return Result.NoContent();

        db.Relationships.Remove(relationship);

        await db.SaveChangesAsync(ct);

        return Result.NoContent();
    }
}