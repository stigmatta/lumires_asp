using FastEndpoints;
using JetBrains.Annotations;
using lumires.Core.Abstractions.Services;

namespace lumires.Api.Features.Relationships.UnblockUser;

[UsedImplicitly]
internal sealed record Command(Guid TargetUserId);

internal sealed class Endpoint(
    ICurrentUserService currentUserService,
    DataAccess db)
    : Endpoint<Command, EmptyResponse>
{
    public override void Configure()
    {
        Post("/users/{targetUserId}/unblock");
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

        var result = await db.UnblockUserAsync(command, currentUserId, ct);

        if (!result.IsSuccess)
        {
            await HttpContext.SendErrorAsync(result.Status, ct);
            return;
        }

        await Send.NoContentAsync(ct);
    }
}