using FastEndpoints;
using JetBrains.Annotations;
using lumires.Core.Abstractions.Services;
using lumires.Domain.Enums;

namespace lumires.Api.Features.Relationships.UnfollowUser;

[UsedImplicitly]
internal sealed record Command(Guid TargetUserId);

[UsedImplicitly]
internal sealed record Response(UserRelationshipStatus Status, UserRelationshipType Type);

internal sealed class Endpoint(
    ICurrentUserService currentUserService,
    DataAccess db)
    : Endpoint<Command, Response>
{
    public override void Configure()
    {
        Post("/users/{targetUserId}/unfollow");
        Description(x => x.WithTags("Relationships"));
        Throttle(5, 2);
    }

    public override async Task HandleAsync(Command command, CancellationToken ct)
    {
        var currentUserId = currentUserService.UserId;

        if (currentUserId == command.TargetUserId)
        {
            await Send.NoContentAsync(ct);
            return;
        }

        var result = await db.UnfollowUserAsync(command, currentUserId, ct);

        if (!result.IsSuccess)
        {
            await HttpContext.SendErrorAsync(result.Status, ct);
            return;
        }

        await Send.NoContentAsync(ct);
    }
}