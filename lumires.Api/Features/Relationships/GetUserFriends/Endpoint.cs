using FastEndpoints;
using JetBrains.Annotations;
using lumires.Core.Abstractions.Services;
using lumires.Domain.Enums;

namespace lumires.Api.Features.Relationships.GetUserFriends;

[UsedImplicitly]
internal sealed record Query(string Username);

[UsedImplicitly]
internal sealed record FriendItem(
    Guid RelationshipId,
    Guid SourceUserId,
    Guid TargetUserId,
    Guid OtherUserId,
    string Username,
    string? AvatarUrl,
    UserRelationshipStatus Status,
    UserRelationshipType Type,
    int FollowerCount
);

[UsedImplicitly]
internal sealed record Response(
    List<FriendItem> Friends,
    int TotalFollowers,
    int TotalFollowings,
    int TotalFriends,
    bool IsMe);

internal sealed class Endpoint(
    ICurrentUserService currentUserService,
    DataAccess db) : Endpoint<Query, Response>
{
    public override void Configure()
    {
        Get("/users/{username}/friends");
        Description(x => x.WithTags("Relationship"));
    }

    public override async Task HandleAsync(Query query, CancellationToken ct)
    {
        var currentUserId = currentUserService.UserId;

        var result = await db.GetUserFriends(query.Username, currentUserId, ct);
        if (!result.IsSuccess)
        {
            await HttpContext.SendErrorAsync(result.Status, ct);
            return;
        }

        await Send.OkAsync(result, ct);
    }
}