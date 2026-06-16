using Ardalis.Result;
using JetBrains.Annotations;
using lumires.Core.Abstractions.Data;
using lumires.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace lumires.Api.Features.Relationships.GetUserFriends;

[UsedImplicitly]
internal sealed class DataAccess(IAppDbContext db) : IDataAccess
{
    public async Task<Result<Response>> GetUserFriends(string username, Guid currentUserId, CancellationToken ct)
    {
        var user = await db.Users
            .FirstOrDefaultAsync(u => u.Username == username, ct);

        if (user is null) return Result.NotFound();

        var relationships = await db.Relationships
            .Where(r => r.Type == UserRelationshipType.Follow
                        && r.Status == UserRelationshipStatus.Accepted
                        && (r.SourceUserId == user.Id || r.TargetUserId == user.Id))
            .Select(r => new FriendItem(
                r.Id,
                r.SourceUserId,
                r.TargetUserId,
                r.SourceUserId == user.Id ? r.TargetUserId : r.SourceUserId,
                r.SourceUserId == user.Id ? r.TargetUser.Username : r.SourceUser.Username,
                r.SourceUserId == user.Id ? r.TargetUser.AvatarUrl : r.SourceUser.AvatarUrl,
                r.Status,
                r.Type,
                r.SourceUserId == user.Id
                    ? r.TargetUser.IncomingRelationships
                        .Count(x => x.Status == UserRelationshipStatus.Accepted &&
                                    x.Type == UserRelationshipType.Follow)
                    : r.SourceUser.IncomingRelationships
                        .Count(x => x.Status == UserRelationshipStatus.Accepted &&
                                    x.Type == UserRelationshipType.Follow)
            ))
            .ToListAsync(ct);


        var followers = relationships
            .Where(r => r.TargetUserId == user.Id)
            .Select(r => r.SourceUserId)
            .ToHashSet();

        var following = relationships
            .Where(r => r.SourceUserId == user.Id)
            .Select(r => r.TargetUserId)
            .ToHashSet();

        var friends = followers.Intersect(following).ToList();

        var response = new Response(
            relationships,
            followers.Count,
            following.Count,
            friends.Count,
            user.Id == currentUserId
        );

        return Result.Success(response);
    }
}